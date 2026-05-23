import { create } from 'zustand';
import type { AuthResponse, HospitalSearchResult, ServiceFilter } from '../types';
import { clearTokens, setTokens } from '../services/api';

interface AuthState {
  user: { username: string; role: 'SuperAdmin' | 'HospitalAdmin'; hospitalId: number | null } | null;
  setAuth: (a: AuthResponse) => void;
  logout: () => void;
  hydrate: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  setAuth: (a) => {
    setTokens(a.accessToken, a.refreshToken);
    const u = { username: a.username, role: a.role, hospitalId: a.hospitalId };
    localStorage.setItem('mediroute_user', JSON.stringify(u));
    set({ user: u });
  },
  logout: () => {
    clearTokens();
    localStorage.removeItem('mediroute_user');
    set({ user: null });
  },
  hydrate: () => {
    const raw = localStorage.getItem('mediroute_user');
    if (raw) {
      try { set({ user: JSON.parse(raw) }); } catch { /* noop */ }
    }
  },
}));

interface HospitalState {
  results: HospitalSearchResult[];
  loading: boolean;
  error: string | null;
  service: ServiceFilter;
  specialization: string;
  radius: number;
  userLocation: { lat: number; lng: number } | null;
  selectedHospitalId: number | null;

  setResults: (r: HospitalSearchResult[]) => void;
  patchHospitalAvailability: (hospitalId: number, availability: HospitalSearchResult['availability']) => void;
  setLoading: (b: boolean) => void;
  setError: (e: string | null) => void;
  setService: (s: ServiceFilter) => void;
  setSpecialization: (s: string) => void;
  setRadius: (n: number) => void;
  setLocation: (loc: { lat: number; lng: number } | null) => void;
  setSelected: (id: number | null) => void;
}

export const useHospitalStore = create<HospitalState>((set) => ({
  results: [],
  loading: false,
  error: null,
  service: '',
  specialization: '',
  radius: 15,
  userLocation: null,
  selectedHospitalId: null,

  setResults: (r) => set({ results: r }),
  patchHospitalAvailability: (hospitalId, availability) =>
    set((s) => ({
      results: s.results.map((h) =>
        h.id === hospitalId ? { ...h, availability } : h
      ),
    })),
  setLoading: (loading) => set({ loading }),
  setError: (error) => set({ error }),
  setService: (service) => set({ service }),
  setSpecialization: (specialization) => set({ specialization }),
  setRadius: (radius) => set({ radius }),
  setLocation: (userLocation) => set({ userLocation }),
  setSelected: (selectedHospitalId) => set({ selectedHospitalId }),
}));
