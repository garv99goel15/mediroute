import { useCallback } from 'react';
import { hospitalApi } from '../services/api';
import { useHospitalStore } from '../store/hospitalStore';

export function useHospitalSearch() {
  const {
    service, specialization, radius, userLocation,
    setResults, setLoading, setError,
  } = useHospitalStore();

  const search = useCallback(async (override?: { lat: number; lng: number }) => {
    const loc = override ?? userLocation;
    if (!loc) {
      setError('Location not available. Allow geolocation or use the default location.');
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const results = await hospitalApi.search(
        loc.lat, loc.lng,
        service || undefined,
        specialization || undefined,
        radius
      );
      setResults(results);
    } catch (e: any) {
      setError(e?.response?.data?.error ?? e?.message ?? 'Search failed');
    } finally {
      setLoading(false);
    }
  }, [service, specialization, radius, userLocation, setResults, setLoading, setError]);

  return { search };
}
