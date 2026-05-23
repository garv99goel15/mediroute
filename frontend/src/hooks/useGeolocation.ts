import { useCallback, useEffect, useState } from 'react';

function friendlyGeoError(err: GeolocationPositionError): string {
  if (err.code === err.PERMISSION_DENIED) return 'Location permission denied';
  if (err.code === err.POSITION_UNAVAILABLE) return 'Location unavailable';
  if (err.code === err.TIMEOUT) return 'Location request timed out';
  return err.message;
}

interface GeoState {
  coords: { lat: number; lng: number } | null;
  error: string | null;
  loading: boolean;
  retry: () => void;
}

/**
 * Browser geolocation hook. Falls back gracefully if denied —
 * caller should provide a sensible default (e.g. central Delhi: 28.6139, 77.2090).
 * Exposes `retry()` to re-request permission.
 */
export function useGeolocation(): GeoState {
  const [state, setState] = useState<Omit<GeoState, 'retry'>>({
    coords: null, error: null, loading: true,
  });

  const request = useCallback(() => {
    if (!('geolocation' in navigator)) {
      setState({ coords: null, error: 'Geolocation not supported by this browser', loading: false });
      return;
    }
    setState((s) => ({ ...s, loading: true, error: null }));
    navigator.geolocation.getCurrentPosition(
      (pos) => setState({
        coords: { lat: pos.coords.latitude, lng: pos.coords.longitude },
        error: null,
        loading: false,
      }),
      (err) => {
        setState({ coords: null, error: friendlyGeoError(err), loading: false });
      },
      { enableHighAccuracy: true, timeout: 10000, maximumAge: 60_000 }
    );
  }, []);

  useEffect(() => { request(); }, [request]);

  return { ...state, retry: request };
}
