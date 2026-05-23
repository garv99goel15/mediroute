import axios, { AxiosInstance, AxiosError } from 'axios';
import type {
  AuthResponse,
  HospitalSearchResult,
  Hospital,
  Availability,
  Doctor,
  AdminDashboard,
} from '../types';

const BASE = (import.meta.env.VITE_API_BASE_URL as string) || 'http://localhost:5000';
const TOKEN_KEY = 'mediroute_access_token';
const REFRESH_KEY = 'mediroute_refresh_token';

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}
export function getRefreshToken(): string | null {
  return localStorage.getItem(REFRESH_KEY);
}
export function setTokens(access: string, refresh: string) {
  localStorage.setItem(TOKEN_KEY, access);
  localStorage.setItem(REFRESH_KEY, refresh);
}
export function clearTokens() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(REFRESH_KEY);
}

export const api: AxiosInstance = axios.create({
  baseURL: BASE,
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((cfg) => {
  const t = getToken();
  if (t) cfg.headers.Authorization = `Bearer ${t}`;
  return cfg;
});

let refreshing: Promise<string | null> | null = null;

api.interceptors.response.use(
  (r) => r,
  async (err: AxiosError) => {
    const original: any = err.config;
    if (err.response?.status === 401 && !original?._retry && getRefreshToken()) {
      original._retry = true;
      try {
        refreshing ??= (async () => {
          const r = await axios.post<AuthResponse>(`${BASE}/api/auth/refresh`, {
            refreshToken: getRefreshToken(),
          });
          setTokens(r.data.accessToken, r.data.refreshToken);
          return r.data.accessToken;
        })();
        const newToken = await refreshing;
        refreshing = null;
        if (newToken) {
          original.headers.Authorization = `Bearer ${newToken}`;
          return api(original);
        }
      } catch {
        refreshing = null;
        clearTokens();
      }
    }
    return Promise.reject(err);
  }
);

// ---------------- Public ----------------
export const authApi = {
  login: (username: string, password: string) =>
    api.post<AuthResponse>('/api/auth/login', { username, password }).then((r) => r.data),
};

export const hospitalApi = {
  search: (lat: number, lng: number, service?: string, specialization?: string, radius = 10) =>
    api
      .get<HospitalSearchResult[]>('/api/hospitals/search', {
        params: { lat, lng, service, specialization, radius },
      })
      .then((r) => r.data),

  getById: (id: number) => api.get<Hospital>(`/api/hospitals/${id}`).then((r) => r.data),

  availability: (id: number) =>
    api.get<Availability>(`/api/hospitals/${id}/availability`).then((r) => r.data),

  doctors: (id: number, specialization?: string) =>
    api
      .get<Doctor[]>(`/api/hospitals/${id}/doctors`, { params: { specialization } })
      .then((r) => r.data),

  nearby: (lat: number, lng: number, radius = 5) =>
    api.get<any[]>('/api/hospitals/nearby', { params: { lat, lng, radius } }).then((r) => r.data),

  snapshotHistory: (id: number, hours = 24) =>
    api.get<any[]>(`/api/hospitals/${id}/snapshot-history`, { params: { hours } }).then((r) => r.data),
};

// ---------------- Admin ----------------
export const adminApi = {
  dashboard: (hospitalId?: number) =>
    api.get<AdminDashboard>('/api/admin/dashboard', { params: { hospitalId } }).then((r) => r.data),

  snapshot: (hospitalId?: number) =>
    api.post('/api/admin/snapshot', null, { params: { hospitalId } }),

  updateBed: (bedId: number, status: string) =>
    api.put(`/api/beds/${bedId}/status`, { status }),

  bulkUpdateBeds: (updates: { bedId: number; status: string }[]) =>
    api.put('/api/beds/bulk-update', { updates }),

  toggleDoctor: (doctorId: number, isAvailable: boolean, nextAvailableAt?: string | null) =>
    api.put(`/api/doctors/${doctorId}/availability`, { isAvailable, nextAvailableAt }),

  updateAmbulance: (ambulanceId: number, status: string) =>
    api.put(`/api/ambulances/${ambulanceId}/status`, { status }),

  updateOt: (departmentId: number, isActive: boolean, availableRooms?: number) =>
    api.put(`/api/ot/${departmentId}/status`, { isActive, availableRooms }),
};
