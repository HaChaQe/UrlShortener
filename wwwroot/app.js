// API Base URL
const API_BASE = '/api';

// Auth token
let authToken = localStorage.getItem('authToken');
let currentUser = null;
let currentPage = 1;
let currentSearch = '';

// FONKSİYONLARI GLOBAL SCOPE'A EKLE
window.showLogin = showLogin;
window.showRegister = showRegister;
window.showDashboard = showDashboard;
window.showHome = showHome;
window.logout = logout;
window.shortenUrl = shortenUrl;
window.copyToClipboard = copyToClipboard;
window.copyUrlToClipboard = copyUrlToClipboard;
window.searchUrls = searchUrls;
window.showUrlStats = showUrlStats;
window.showEditUrl = showEditUrl;
window.deleteUrl = deleteUrl;
window.loadUserUrls = loadUserUrls;

// Initialize app
document.addEventListener('DOMContentLoaded', function () {
    console.log('DOM loaded - setting up event listeners');

    if (authToken) {
        checkAuth();
    }

    // Form event listeners - güvenli kontrol
    const loginForm = document.getElementById('loginForm');
    const registerForm = document.getElementById('registerForm');
    const editForm = document.getElementById('editForm');
    const searchInput = document.getElementById('searchInput');

    if (loginForm) {
        loginForm.addEventListener('submit', handleLogin);
    }

    if (registerForm) {
        registerForm.addEventListener('submit', handleRegister);
    }

    if (editForm) {
        editForm.addEventListener('submit', handleEditUrl);
    }

    // Search input
    if (searchInput) {
        searchInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                searchUrls();
            }
        });
    }

    console.log('Event listeners attached successfully');
});

// Authentication functions
async function handleLogin(e) {
    e.preventDefault();

    const email = document.getElementById('loginEmail').value;
    const password = document.getElementById('loginPassword').value;

    try {
        const response = await fetch(`${API_BASE}/auth/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ email, password })
        });

        const result = await response.json();

        if (response.ok) {
            authToken = result.data.token;
            currentUser = result.data;
            localStorage.setItem('authToken', authToken);

            hideModals();
            showUserInterface();
            showNotification('Başarıyla giriş yaptınız!', 'success');
        } else {
            showNotification(result.message || 'Giriş yapılamadı', 'error');
        }
    } catch (error) {
        console.error('Login error:', error);
        showNotification('Bir hata oluştu', 'error');
    }
}

async function handleRegister(e) {
    e.preventDefault();

    const fullName = document.getElementById('registerFullName').value;
    const email = document.getElementById('registerEmail').value;
    const password = document.getElementById('registerPassword').value;
    const confirmPassword = document.getElementById('registerConfirmPassword').value;

    if (password !== confirmPassword) {
        showNotification('Şifreler eşleşmiyor', 'error');
        return;
    }

    try {
        const response = await fetch(`${API_BASE}/auth/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ fullName, email, password, confirmPassword })
        });

        const result = await response.json();

        if (response.ok) {
            authToken = result.data.token;
            currentUser = result.data;
            localStorage.setItem('authToken', authToken);

            hideModals();
            showUserInterface();
            showNotification('Başarıyla kayıt oldunuz!', 'success');
        } else {
            showNotification(result.message || 'Kayıt olunamadı', 'error');
        }
    } catch (error) {
        console.error('Register error:', error);
        showNotification('Bir hata oluştu', 'error');
    }
}

async function checkAuth() {
    try {
        const response = await fetch(`${API_BASE}/url?page=1&pageSize=1`, {
            headers: {
                'Authorization': `Bearer ${authToken}`
            }
        });

        if (response.ok) {
            showUserInterface();
        } else {
            logout();
        }
    } catch (error) {
        logout();
    }
}

function logout() {
    authToken = null;
    currentUser = null;
    localStorage.removeItem('authToken');
    showGuestInterface();
    showHome();
    showNotification('Çıkış yaptınız', 'info');
}

// UI functions
function showLogin() {
    hideModals();
    new bootstrap.Modal(document.getElementById('loginModal')).show();
}

function showRegister() {
    hideModals();
    new bootstrap.Modal(document.getElementById('registerModal')).show();
}

function hideModals() {
    const modals = document.querySelectorAll('.modal');
    modals.forEach(modal => {
        const bsModal = bootstrap.Modal.getInstance(modal);
        if (bsModal) bsModal.hide();
    });
}

function showUserInterface() {
    const authButtons = document.getElementById('authButtons');
    const userInfo = document.getElementById('userInfo');
    const userName = document.getElementById('userName');

    if (authButtons) authButtons.style.display = 'none';
    if (userInfo) userInfo.style.display = 'block';

    if (currentUser && userName) {
        userName.textContent = currentUser.fullName || currentUser.email;
    }
}

function showGuestInterface() {
    const authButtons = document.getElementById('authButtons');
    const userInfo = document.getElementById('userInfo');

    if (authButtons) authButtons.style.display = 'block';
    if (userInfo) userInfo.style.display = 'none';
}

function showHome() {
    const homePage = document.getElementById('homePage');
    const dashboardPage = document.getElementById('dashboardPage');

    if (homePage) homePage.style.display = 'block';
    if (dashboardPage) dashboardPage.style.display = 'none';
    clearForm();
}

function showDashboard() {
    if (!authToken) {
        showLogin();
        return;
    }

    const homePage = document.getElementById('homePage');
    const dashboardPage = document.getElementById('dashboardPage');

    if (homePage) homePage.style.display = 'none';
    if (dashboardPage) dashboardPage.style.display = 'block';
    loadUserUrls();
}

function clearForm() {
    const elements = [
        'originalUrl', 'title', 'customCode', 'description', 'expiresAt'
    ];

    elements.forEach(id => {
        const element = document.getElementById(id);
        if (element) element.value = '';
    });

    const resultSection = document.getElementById('resultSection');
    if (resultSection) resultSection.style.display = 'none';
}

// URL functions
async function shortenUrl() {
    if (!authToken) {
        showLogin();
        return;
    }

    const originalUrl = document.getElementById('originalUrl').value;
    const title = document.getElementById('title').value;
    const customCode = document.getElementById('customCode').value;
    const description = document.getElementById('description').value;
    const expiresAt = document.getElementById('expiresAt').value;

    if (!originalUrl) {
        showNotification('URL gereklidir', 'error');
        return;
    }

    try {
        const requestBody = {
            originalUrl,
            title: title || null,
            customCode: customCode || null,
            description: description || null,
            expiresAt: expiresAt || null
        };

        const response = await fetch(`${API_BASE}/url`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${authToken}`
            },
            body: JSON.stringify(requestBody)
        });

        const result = await response.json();

        if (response.ok) {
            const baseUrl = window.location.origin;
            const shortUrlResult = document.getElementById('shortUrlResult');
            const resultSection = document.getElementById('resultSection');

            if (shortUrlResult) {
                shortUrlResult.value = `${baseUrl}/${result.data.shortCode}`;
            }
            if (resultSection) {
                resultSection.style.display = 'block';
            }

            showNotification('URL başarıyla kısaltıldı!', 'success');
        } else {
            showNotification(result.message || 'URL kısaltılamadı', 'error');
        }
    } catch (error) {
        console.error('Shorten URL error:', error);
        showNotification('Bir hata oluştu', 'error');
    }
}

async function loadUserUrls(page = 1, search = '') {
    currentPage = page;
    currentSearch = search;

    try {
        const url = `${API_BASE}/url?page=${page}&pageSize=10${search ? `&search=${encodeURIComponent(search)}` : ''}`;
        const response = await fetch(url, {
            headers: {
                'Authorization': `Bearer ${authToken}`
            }
        });

        const result = await response.json();

        if (response.ok) {
            displayUrls(result.data);
            displayPagination(result.data);
        } else {
            showNotification('URL\'ler yüklenemedi', 'error');
        }
    } catch (error) {
        console.error('Load URLs error:', error);
        showNotification('Bir hata oluştu', 'error');
    }
}

function displayUrls(data) {
    const container = document.getElementById('urlsList');
    if (!container) return;

    if (data.data.length === 0) {
        container.innerHTML = '<div class="text-center text-muted">Henüz URL\'niz bulunmuyor.</div>';
        return;
    }

    let html = '';
    data.data.forEach(url => {
        const createdAt = new Date(url.createdAt).toLocaleDateString('tr-TR');
        const expiresAt = url.expiresAt ? new Date(url.expiresAt).toLocaleDateString('tr-TR') : 'Süresiz';
        const statusBadge = url.isActive ?
            '<span class="badge bg-success">Aktif</span>' :
            '<span class="badge bg-danger">Deaktif</span>';

        const baseUrl = window.location.origin;

        html += `
            <div class="url-item">
                <div class="row align-items-center">
                    <div class="col-md-8">
                        <div class="url-title">${url.title || 'Başlıksız'}</div>
                        <div class="text-muted small mb-1">${url.originalUrl}</div>
                        <div class="url-code mb-2">
                            <a href="${baseUrl}/${url.shortCode}" target="_blank">${baseUrl}/${url.shortCode}</a>
                        </div>
                        <div class="url-stats">
                            <i class="fas fa-mouse-pointer"></i> ${url.clickCount} tıklama • 
                            <i class="fas fa-calendar"></i> ${createdAt} • 
                            <i class="fas fa-clock"></i> ${expiresAt} • 
                            ${statusBadge}
                        </div>
                    </div>
                    <div class="col-md-4 text-end">
                        <div class="btn-group-vertical btn-group-sm">
                            <button class="btn btn-outline-primary" onclick="copyUrlToClipboard('${baseUrl}/${url.shortCode}')">
                                <i class="fas fa-copy"></i> Kopyala
                            </button>
                            <button class="btn btn-outline-info" onclick="showUrlStats(${url.id})">
                                <i class="fas fa-chart-bar"></i> İstatistikler
                            </button>
                            <button class="btn btn-outline-secondary" onclick="showEditUrl(${url.id})">
                                <i class="fas fa-edit"></i> Düzenle
                            </button>
                            <button class="btn btn-outline-danger" onclick="deleteUrl(${url.id})">
                                <i class="fas fa-trash"></i> Sil
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;
    });

    container.innerHTML = html;
}

function displayPagination(data) {
    const container = document.getElementById('pagination');
    if (!container) return;

    let html = '';

    if (data.totalPages > 1) {
        // Previous button
        if (data.hasPreviousPage) {
            html += `<li class="page-item">
                <a class="page-link" href="#" onclick="loadUserUrls(${data.page - 1}, '${currentSearch}')">Önceki</a>
            </li>`;
        }

        // Page numbers
        for (let i = 1; i <= data.totalPages; i++) {
            const activeClass = i === data.page ? 'active' : '';
            html += `<li class="page-item ${activeClass}">
                <a class="page-link" href="#" onclick="loadUserUrls(${i}, '${currentSearch}')">${i}</a>
            </li>`;
        }

        // Next button
        if (data.hasNextPage) {
            html += `<li class="page-item">
                <a class="page-link" href="#" onclick="loadUserUrls(${data.page + 1}, '${currentSearch}')">Sonraki</a>
            </li>`;
        }
    }

    container.innerHTML = html;
}

function searchUrls() {
    const searchInput = document.getElementById('searchInput');
    if (!searchInput) return;

    const search = searchInput.value.trim();
    loadUserUrls(1, search);
}

async function showUrlStats(urlId) {
    try {
        const response = await fetch(`${API_BASE}/url/${urlId}/stats`, {
            headers: {
                'Authorization': `Bearer ${authToken}`
            }
        });

        const result = await response.json();

        if (response.ok) {
            displayUrlStats(result.data);
            const statsModal = document.getElementById('statsModal');
            if (statsModal) {
                new bootstrap.Modal(statsModal).show();
            }
        } else {
            showNotification('İstatistikler yüklenemedi', 'error');
        }
    } catch (error) {
        console.error('Stats error:', error);
        showNotification('Bir hata oluştu', 'error');
    }
}

function displayUrlStats(stats) {
    const container = document.getElementById('statsContent');
    if (!container) return;

    let html = `
        <div class="stats-card text-center">
            <div class="stats-number">${stats.totalClicks}</div>
            <div>Toplam Tıklama</div>
        </div>
        
        <div class="row">
            <div class="col-md-6">
                <h6>Günlük Tıklamalar</h6>
                <div class="table-responsive">
                    <table class="table table-sm">
                        <thead>
                            <tr>
                                <th>Tarih</th>
                                <th>Tıklama</th>
                            </tr>
                        </thead>
                        <tbody>
    `;

    if (stats.dailyClicks && stats.dailyClicks.length > 0) {
        stats.dailyClicks.slice(-7).forEach(day => {
            const date = new Date(day.date).toLocaleDateString('tr-TR');
            html += `
                <tr>
                    <td>${date}</td>
                    <td>${day.clicks}</td>
                </tr>
            `;
        });
    } else {
        html += '<tr><td colspan="2" class="text-center text-muted">Henüz tıklama yok</td></tr>';
    }

    html += `
                        </tbody>
                    </table>
                </div>
            </div>
            <div class="col-md-6">
                <h6>Ülke İstatistikleri</h6>
                <div class="table-responsive">
                    <table class="table table-sm">
                        <thead>
                            <tr>
                                <th>Ülke</th>
                                <th>Tıklama</th>
                            </tr>
                        </thead>
                        <tbody>
    `;

    if (stats.countryStats && stats.countryStats.length > 0) {
        stats.countryStats.slice(0, 5).forEach(country => {
            html += `
                <tr>
                    <td>${country.country}</td>
                    <td>${country.clicks}</td>
                </tr>
            `;
        });
    } else {
        html += '<tr><td colspan="2" class="text-center text-muted">Henüz tıklama yok</td></tr>';
    }

    html += `
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    `;

    container.innerHTML = html;
}

async function showEditUrl(urlId) {
    try {
        const response = await fetch(`${API_BASE}/url/${urlId}`, {
            headers: {
                'Authorization': `Bearer ${authToken}`
            }
        });

        const result = await response.json();

        if (response.ok) {
            const url = result.data;
            const elements = {
                editUrlId: url.id,
                editTitle: url.title || '',
                editDescription: url.description || '',
                editIsActive: url.isActive
            };

            Object.entries(elements).forEach(([id, value]) => {
                const element = document.getElementById(id);
                if (element) {
                    if (element.type === 'checkbox') {
                        element.checked = value;
                    } else {
                        element.value = value;
                    }
                }
            });

            if (url.expiresAt) {
                const date = new Date(url.expiresAt);
                const editExpiresAt = document.getElementById('editExpiresAt');
                if (editExpiresAt) {
                    editExpiresAt.value = date.toISOString().slice(0, 16);
                }
            }

            const editModal = document.getElementById('editModal');
            if (editModal) {
                new bootstrap.Modal(editModal).show();
            }
        } else {
            showNotification('URL bilgileri yüklenemedi', 'error');
        }
    } catch (error) {
        console.error('Load URL error:', error);
        showNotification('Bir hata oluştu', 'error');
    }
}

async function handleEditUrl(e) {
    e.preventDefault();

    const urlId = document.getElementById('editUrlId').value;
    const title = document.getElementById('editTitle').value;
    const description = document.getElementById('editDescription').value;
    const expiresAt = document.getElementById('editExpiresAt').value;
    const isActive = document.getElementById('editIsActive').checked;

    try {
        const requestBody = {
            title: title || null,
            description: description || null,
            expiresAt: expiresAt || null,
            isActive
        };

        const response = await fetch(`${API_BASE}/url/${urlId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${authToken}`
            },
            body: JSON.stringify(requestBody)
        });

        const result = await response.json();

        if (response.ok) {
            hideModals();
            loadUserUrls(currentPage, currentSearch);
            showNotification('URL başarıyla güncellendi!', 'success');
        } else {
            showNotification(result.message || 'URL güncellenemedi', 'error');
        }
    } catch (error) {
        console.error('Update URL error:', error);
        showNotification('Bir hata oluştu', 'error');
    }
}

async function deleteUrl(urlId) {
    if (!confirm('Bu URL\'yi silmek istediğinizden emin misiniz?')) {
        return;
    }

    try {
        const response = await fetch(`${API_BASE}/url/${urlId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${authToken}`
            }
        });

        const result = await response.json();

        if (response.ok) {
            loadUserUrls(currentPage, currentSearch);
            showNotification('URL başarıyla silindi!', 'success');
        } else {
            showNotification(result.message || 'URL silinemedi', 'error');
        }
    } catch (error) {
        console.error('Delete URL error:', error);
        showNotification('Bir hata oluştu', 'error');
    }
}

// Utility functions
function copyToClipboard() {
    const shortUrl = document.getElementById('shortUrlResult');
    if (!shortUrl) return;

    shortUrl.select();
    shortUrl.setSelectionRange(0, 99999);
    document.execCommand('copy');
    showNotification('Kısa URL kopyalandı!', 'success');
}

function copyUrlToClipboard(url) {
    if (navigator.clipboard && window.isSecureContext) {
        navigator.clipboard.writeText(url).then(() => {
            showNotification('URL kopyalandı!', 'success');
        }).catch(() => {
            fallbackCopyTextToClipboard(url);
        });
    } else {
        fallbackCopyTextToClipboard(url);
    }
}

function fallbackCopyTextToClipboard(text) {
    const textArea = document.createElement('textarea');
    textArea.value = text;
    textArea.style.position = 'fixed';
    textArea.style.left = '-999999px';
    textArea.style.top = '-999999px';
    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();

    try {
        document.execCommand('copy');
        showNotification('URL kopyalandı!', 'success');
    } catch (err) {
        console.error('Fallback: Oops, unable to copy', err);
        showNotification('Kopyalama başarısız!', 'error');
    }

    document.body.removeChild(textArea);
}

function showNotification(message, type = 'info') {
    const alertClass = type === 'success' ? 'alert-success' :
        type === 'error' ? 'alert-danger' :
            type === 'warning' ? 'alert-warning' : 'alert-info';

    const notification = document.createElement('div');
    notification.className = `alert ${alertClass} alert-dismissible fade show position-fixed`;
    notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    document.body.appendChild(notification);

    setTimeout(() => {
        if (notification.parentNode) {
            notification.parentNode.removeChild(notification);
        }
    }, 5000);
}

// Navigation functions
function navigateToDashboard() {
    if (authToken) {
        showDashboard();
    } else {
        showLogin();
    }
}

// Auto-login check on page load
if (authToken) {
    // checkAuth(); // Bu DOMContentLoaded'da çağrılıyor
}