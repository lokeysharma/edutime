import axios from 'axios';

const API_BASE_URL = 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add token to requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Handle 401 errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Auth APIs
export const authApi = {
  login: (email: string, password: string) =>
    api.post('/Auth/login', { email, password }),
  register: (data: any) => api.post('/Auth/register', data),
  changePassword: (currentPassword: string, newPassword: string) =>
    api.post('/Auth/change-password', { currentPassword, newPassword }),
};

// Users APIs
export const usersApi = {
  getAll: () => api.get('/Users'),
  getById: (id: number) => api.get(`/Users/${id}`),
  getMe: () => api.get('/Users/me'),
  getTeachers: () => api.get('/Users/teachers'),
  getStudents: () => api.get('/Users/students'),
  update: (id: number, data: any) => api.put(`/Users/${id}`, data),
  delete: (id: number) => api.delete(`/Users/${id}`),
};

// Courses APIs
export const coursesApi = {
  getAll: () => api.get('/Courses'),
  getById: (id: number) => api.get(`/Courses/${id}`),
  getMyCourses: () => api.get('/Courses/my-courses'),
  create: (data: any) => api.post('/Courses', data),
  update: (id: number, data: any) => api.put(`/Courses/${id}`, data),
  delete: (id: number) => api.delete(`/Courses/${id}`),
};

// Sessions APIs
export const sessionsApi = {
  getAll: () => api.get('/ClassSession'),
  getById: (id: number) => api.get(`/ClassSession/${id}`),
  getByCourse: (courseId: number) => api.get(`/ClassSession/course/${courseId}`),
  create: (data: any) => api.post('/ClassSession', data),
  update: (id: number, data: any) => api.put(`/ClassSession/${id}`, data),
  delete: (id: number) => api.delete(`/ClassSession/${id}`),
};

// Enrollments APIs
export const enrollmentsApi = {
  getAll: () => api.get('/Enrollments'),
  getMyEnrollments: () => api.get('/Enrollments/my-enrollments'),
  create: (data: any) => api.post('/Enrollments', data),
  update: (id: number, status: string) => api.put(`/Enrollments/${id}`, JSON.stringify(status), {
    headers: { 'Content-Type': 'application/json' }
  }),
  delete: (id: number) => api.delete(`/Enrollments/${id}`),
};

// Assignments APIs
export const assignmentsApi = {
  getAll: () => api.get('/Assignments'),
  getById: (id: number) => api.get(`/Assignments/${id}`),
  getMyAssignments: () => api.get('/Assignments/my-assignments'),
  create: (data: any) => api.post('/Assignments', data),
  update: (id: number, data: any) => api.put(`/Assignments/${id}`, data),
  delete: (id: number) => api.delete(`/Assignments/${id}`),
};

// Submissions APIs
export const submissionsApi = {
  getByAssignment: (assignmentId: number) => api.get(`/Submissions/assignment/${assignmentId}`),
  getMySubmissions: () => api.get('/Submissions/my-submissions'),
  submit: (data: any) => api.post('/Submissions', data),
  grade: (id: number, data: any) => api.put(`/Submissions/${id}/grade`, data),
};

// Attendance APIs
export const attendanceApi = {
  getBySession: (sessionId: number) => api.get(`/Attendance/session/${sessionId}`),
  getMyAttendance: () => api.get('/Attendance/my-attendance'),
  record: (data: any) => api.post('/Attendance', data),
  bulkRecord: (data: any[]) => api.post('/Attendance/bulk', data),
};

export default api;
