    // API Configuration
const API_BASE_URL = 'http://localhost:5221/api';

// Auth Helper Functions
const auth = {
    // Get token from storage
    getToken() {
        return localStorage.getItem('authToken') || sessionStorage.getItem('authToken');
    },

    // Set token to storage
    setToken(token, remember = false) {
        if (remember) {
            localStorage.setItem('authToken', token);
        } else {
            sessionStorage.setItem('authToken', token);
        }
    },

    // Remove token from storage
    removeToken() {
        localStorage.removeItem('authToken');
        sessionStorage.removeItem('authToken');
        localStorage.removeItem('currentUser');
        sessionStorage.removeItem('currentUser');
    },

    // Get current user
    getCurrentUser() {
        const userJson = localStorage.getItem('currentUser') || sessionStorage.getItem('currentUser');
        return userJson ? JSON.parse(userJson) : null;
    },

    // Set current user
    setCurrentUser(user, remember = false) {
        const userJson = JSON.stringify(user);
        if (remember) {
            localStorage.setItem('currentUser', userJson);
        } else {
            sessionStorage.setItem('currentUser', userJson);
        }
    },

    // Check if user is logged in
    isLoggedIn() {
        return !!this.getToken();
    },

    // Check if user is admin
    isAdmin() {
        const user = this.getCurrentUser();
        return user && user.role === 'admin';
    },

    // Check if user is customer
    isCustomer() {
        const user = this.getCurrentUser();
        return user && user.role === 'customer';
    },

    // Check if user is shipper
    isShipper() {
        const user = this.getCurrentUser();
        return user && user.role === 'shipper';
    },

    // Logout
    logout() {
        this.removeToken();
        // Redirect to the shared login page at site root so all roles use the same login.
        // Use absolute path to avoid relative path issues from subfolders (shipper/, customer/, etc.).
        try {
            const origin = window.location.origin || (window.location.protocol + '//' + window.location.host);
            window.location.href = origin + '/login.html';
        } catch (e) {
            // fallback to a root-relative path
            window.location.href = '/login.html';
        }
    },

    // Redirect to login if not authenticated
    requireAuth() {
        if (!this.isLoggedIn()) {
            const origin = window.location.origin || (window.location.protocol + '//' + window.location.host);
            window.location.href = origin + '/login.html';
            return false;
        }
        return true;
    }
};

// API Service Class
class ApiService {
    constructor(baseUrl) {
        this.baseUrl = baseUrl;
    }

    // Generic request method with JWT authentication
    async request(endpoint, options = {}) {
        try {
            const url = `${this.baseUrl}${endpoint}`;
            
            // Add Authorization header if token exists
            const token = auth.getToken();
            const headers = {
                'Content-Type': 'application/json',
                ...options.headers
            };
            
            if (token) {
                headers['Authorization'] = `Bearer ${token}`;
            }

            const response = await fetch(url, {
                ...options,
                headers
            });

            // Handle 401 Unauthorized - redirect to login
            if (response.status === 401) {
                auth.removeToken();
                utils.showToast('Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại!', 'warning');
                setTimeout(() => {
                    const origin = window.location.origin || (window.location.protocol + '//' + window.location.host);
                    window.location.href = origin + '/login.html';
                }, 1500);
                throw new Error('Unauthorized');
            }

            // Handle 403 Forbidden
            if (response.status === 403) {
                utils.showToast('Bạn không có quyền thực hiện thao tác này!', 'danger');
                throw new Error('Forbidden');
            }

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            // Check if response has content
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            }
            
            return null;
        } catch (error) {
            console.error('API Request Error:', error);
            throw error;
        }
    }

    // Orders API
    async getAllOrders() {
        return await this.request('/orders');
    }

    async getMyOrders() {
        return await this.request('/orders/my');
    }

    async getOrderById(id) {
        return await this.request(`/orders/${id}`);
    }

    async payOrder(id) {
        return await this.request(`/orders/${id}/pay`, { method: 'POST' });
    }

    async confirmReceived(id) {
        return await this.request(`/orders/${id}/confirm-received`, { method: 'PATCH' });
    }

    async createOrder(orderData) {
        return await this.request('/orders', {
            method: 'POST',
            body: JSON.stringify(orderData)
        });
    }

    // Cập nhật trạng thái đơn hàng, cho phép truyền GPS location (nếu có)
    async updateOrderStatus(id, status, gpsLocation = null) {
        // Nếu gpsLocation có, gửi kèm lên backend
        const body = gpsLocation !== null
            ? JSON.stringify({ status, gpsLocation })
            : JSON.stringify({ status });
        return await this.request(`/orders/${id}/status`, {
            method: 'PATCH',
            body
        });
    }

    async assignStaff(orderId, staffId) {
        return await this.request(`/orders/${orderId}/assign-staff/${staffId}`, {
            method: 'PATCH'
        });
    }

    async getOrdersByStatus(status) {
        return await this.request(`/orders/status/${status}`);
    }

    async getOrdersByStaff(staffId) {
        return await this.request(`/orders/staff/${staffId}`);
    }

    async deleteOrder(id) {
        return await this.request(`/orders/${id}`, {
            method: 'DELETE'
        });
    }

    // Feedback API
    async postFeedback(feedback) {
        return await this.request('/feedback', {
            method: 'POST',
            body: JSON.stringify(feedback)
        });
    }

    async getFeedbacksByOrder(orderId) {
        return await this.request(`/feedback/order/${orderId}`);
    }

    async getMyFeedbacks() {
        return await this.request('/feedback/my');
    }

    // Delivery Staff API
    async getAllStaff() {
        return await this.request('/deliverystaff');
    }

    async getDeliveryStaff() {
        return await this.request('/deliverystaff');
    }

    async getStaffById(id) {
        return await this.request(`/deliverystaff/${id}`);
    }

    async getMyStaffRecord() {
        return await this.request('/deliverystaff/me');
    }

    async getAvailableStaff() {
        return await this.request('/deliverystaff/available');
    }

    async createStaff(staffData) {
        return await this.request('/deliverystaff', {
            method: 'POST',
            body: JSON.stringify(staffData)
        });
    }

    async addDeliveryStaff(staffData) {
        return await this.request('/deliverystaff', {
            method: 'POST',
            body: JSON.stringify(staffData)
        });
    }

    async updateStaffAvailability(id, isAvailable) {
        return await this.request(`/deliverystaff/${id}/availability`, {
            method: 'PATCH',
            body: JSON.stringify(isAvailable)
        });
    }

    async deleteDeliveryStaff(staffId) {
        return await this.request(`/deliverystaff/${staffId}`, {
            method: 'DELETE'
        });
    }

    // Customers (Admin)
    async getAllCustomers() {
        return await this.request('/customers');
    }

    async getCustomerById(id) {
        return await this.request(`/customers/${id}`);
    }

    async createCustomer(data) {
        return await this.request('/customers', {
            method: 'POST',
            body: JSON.stringify(data)
        });
    }

    async updateCustomer(id, data) {
        return await this.request(`/customers/${id}`, {
            method: 'PUT',
            body: JSON.stringify(data)
        });
    }

    async deleteCustomer(id) {
        return await this.request(`/customers/${id}`, { method: 'DELETE' });
    }

    async getStaffOrders(staffId) {
        return await this.request(`/orders/staff/${staffId}`);
    }

    // Tracking API
    async getOrderCheckpoints(orderId) {
        return await this.request(`/tracking/order/${orderId}`);
    }

    async checkIn(checkpointData) {
        return await this.request('/tracking/checkin', {
            method: 'POST',
            body: JSON.stringify(checkpointData)
        });
    }

    async trackByOrderCode(orderCode) {
        return await this.request(`/tracking/track/${orderCode}`);
    }

    async getCurrentLocation(orderId) {
        return await this.request(`/tracking/location/${orderId}`);
    }

        // Auth API
        async login(data) {
            return await this.request('/auth/login', {
                method: 'POST',
                body: JSON.stringify(data)
            });
        }

        async register(data) {
            return await this.request('/auth/register', {
                method: 'POST',
                body: JSON.stringify(data)
            });
        }

        async forgotPassword(data) {
            return await this.request('/auth/forgot-password', {
                method: 'POST',
                body: JSON.stringify(data)
            });
        }

        async resetPassword(data) {
            return await this.request('/auth/reset-password', {
                method: 'POST',
                body: JSON.stringify(data)
            });
        }

        // Profile API
        async getMyProfile() {
            return await this.request('/profile/me');
        }

        async updateMyProfile(data) {
            return await this.request('/profile/me', {
                method: 'PUT',
                body: JSON.stringify(data)
            });
        }

        async changeMyPassword(data) {
            return await this.request('/profile/change-password', {
                method: 'PATCH',
                body: JSON.stringify(data)
            });
        }

        // Users (Admin) API
        async getAllUsers() {
            return await this.request('/users');
        }

        async getUserById(id) {
            return await this.request(`/users/${id}`);
        }

        async createUserAccount(data) {
            return await this.request('/users', {
                method: 'POST',
                body: JSON.stringify(data)
            });
        }

        async updateUserAccount(id, data) {
            return await this.request(`/users/${id}`, {
                method: 'PUT',
                body: JSON.stringify(data)
            });
        }

        async resetUserPassword(id, data) {
            return await this.request(`/users/${id}/password`, {
                method: 'PATCH',
                body: JSON.stringify(data)
            });
        }

        async deleteUserAccount(id) {
            return await this.request(`/users/${id}`, { method: 'DELETE' });
        }

        // Reports API
        async getReportsSummary() {
            return await this.request('/reports/summary');
        }

        // expects query object { days: 30 }
        async getOrdersByDay(query = {}) {
            const q = query.days ? `?days=${query.days}` : '';
            return await this.request(`/reports/orders-by-day${q}`);
        }

        async getOrdersByStaff() {
            return await this.request('/reports/orders-by-staff');
        }

        async getByDeliveryType() {
            return await this.request('/reports/by-delivery-type');
        }

        async getByPackageType() {
            return await this.request('/reports/by-package-type');
        }
}

// Create global API service instance
const apiService = new ApiService(API_BASE_URL);

// Utility Functions
const utils = {
    // Format currency
    formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    },

    // Format date
    formatDate(dateString) {
        if (!dateString) return '-';
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });
    },

    // Get status text
    getStatusText(status) {
        const statusMap = {
            0: 'Chưa Nhận',
            1: 'Đã Nhận - Chưa Giao',
            2: 'Đang Giao',
            3: 'Đã Giao'
        };
        return statusMap[status] || 'Không xác định';
    },

    // Get status badge class
    getStatusClass(status) {
        const classMap = {
            0: 'status-pending',
            1: 'status-received',
            2: 'status-delivering',
            3: 'status-completed'
        };
        return classMap[status] || 'status-pending';
    },

    // Get delivery type text
    getDeliveryTypeText(type) {
        return type === 0 ? 'Giao Thường' : 'Giao Nhanh';
    },

    // Get payment method text
    getPaymentMethodText(method) {
        const methodMap = {
            0: 'COD',
            1: 'Gửi Nhanh',
            2: 'Chuyển Khoản',
            3: 'Thanh Toán Online'
        };
        return methodMap[method] || 'Không xác định';
    },

    // Get package type text
    getPackageTypeText(type) {
        const typeMap = {
            0: 'Gói Nhỏ',
            1: 'Gói Bọc Van',
            2: 'Bọc',
            3: 'Bao',
            4: 'Thùng',
            5: 'Bao PB',
            6: 'Hộp Thùng',
            7: 'TiVi',
            8: 'Laptop',
            9: 'Máy Tính',
            10: 'CPU',
            11: 'Xe'
        };
        return typeMap[type] || 'Không xác định';
    },

    // Show toast notification
    showToast(message, type = 'success') {
        const toastContainer = document.getElementById('toastContainer');
        if (!toastContainer) {
            const container = document.createElement('div');
            container.id = 'toastContainer';
            container.style.cssText = 'position: fixed; top: 20px; right: 20px; z-index: 9999;';
            document.body.appendChild(container);
        }

        const toast = document.createElement('div');
        toast.className = `alert alert-${type} alert-dismissible fade show`;
        toast.role = 'alert';
        toast.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        document.getElementById('toastContainer').appendChild(toast);

        setTimeout(() => {
            toast.remove();
        }, 5000);
    },

    // Show loading spinner
    showLoading(element) {
        element.innerHTML = `
            <div class="text-center py-4">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Đang tải...</span>
                </div>
            </div>
        `;
    },

    // Show error message
    showError(element, message) {
        element.innerHTML = `
            <div class="alert alert-danger" role="alert">
                <i class="fas fa-exclamation-triangle"></i>
                ${message}
            </div>
        `;
    },

    // Show empty message
    showEmpty(element, message) {
        element.innerHTML = `
            <tr>
                <td colspan="10" class="text-center py-4">
                    <i class="fas fa-inbox fa-3x text-muted mb-3"></i>
                    <p class="text-muted">${message}</p>
                </td>
            </tr>
        `;
    },

    // Confirm dialog
    async confirm(message) {
        return confirm(message);
    }
};

// Export for use in other files
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { apiService, utils, auth };
}

// Expose to window so inline handlers (onclick="auth.logout()") can access these
if (typeof window !== 'undefined') {
    window.apiService = apiService;
    window.utils = utils;
    window.auth = auth;
}
