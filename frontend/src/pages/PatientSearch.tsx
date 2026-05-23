import { useEffect, useMemo, useState, lazy, Suspense } from 'react';
import { SearchBar } from '../components/Search/SearchBar';
import { FilterPanel } from '../components/Search/FilterPanel';
import { HospitalCard } from '../components/Hospital/HospitalCard';
import { LoadingSpinner } from '../components/shared/LoadingSpinner';
import { useGeolocation } from '../hooks/useGeolocation';
import { useHospitalStore } from '../store/hospitalStore';
import { useHospitalSearch } from '../hooks/useHospitalSearch';
import { useMultiHospitalSignalR } from '../hooks/useSignalR';

// Lazy: Leaflet + react-leaflet are ~150KB gzipped — only load when user clicks Map.
const HospitalMap = lazy(() =>
  import('../components/Map/HospitalMap').then((m) => ({ default: m.HospitalMap }))
);

const DEFAULT_CENTER = { lat: 28.6139, lng: 77.209 }; // Connaught Place, Delhi

export function PatientSearch() {
  const geo = useGeolocation();
  const [view, setView] = useState<'list' | 'map'>('list');
  const [showBestOnly, setShowBestOnly] = useState(false);
  const {
    results, loading, error, service,
    userLocation, setLocation, patchHospitalAvailability,
  } = useHospitalStore();
  const { search } = useHospitalSearch();

  useEffect(() => {
    if (geo.coords) setLocation(geo.coords);
    else if (!geo.loading) setLocation(DEFAULT_CENTER);
  }, [geo.coords, geo.loading, setLocation]);

  useEffect(() => {
    if (userLocation) void search();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userLocation]);

  const ids = useMemo(() => results.map((r) => r.id), [results]);
  useMultiHospitalSignalR(ids, (e) => patchHospitalAvailability(e.hospitalId, e.availability));

  const bestId = results[0]?.id ?? null;
  const visible = showBestOnly && bestId ? results.filter((r) => r.id === bestId) : results;

  return (
    <div className="mx-auto max-w-7xl px-4 py-6 space-y-4">
      <header>
        <h1 className="text-2xl font-bold text-slate-900">Find the nearest hospital</h1>
        <p className="text-slate-600">Real-time availability across ICU, OT, Emergency & more.</p>
      </header>

      {geo.error && (
        <div className="rounded-lg border border-amber-200 bg-amber-50 p-3 text-sm text-amber-900 flex items-start gap-3">
          <span aria-hidden="true" className="mt-0.5 text-base">📍</span>
          <div className="flex-1">
            <div className="font-medium">{geo.error}</div>
            <div className="text-xs text-amber-800/90 mt-0.5">
              Using <strong>central Delhi</strong> as the search center. Allow location access in your browser&apos;s address bar for results near you, then click Retry.
            </div>
          </div>
          <button
            onClick={geo.retry}
            disabled={geo.loading}
            className="shrink-0 rounded-md border border-amber-300 bg-white px-3 py-1.5 text-xs font-medium text-amber-900 hover:bg-amber-100 disabled:opacity-60"
          >{geo.loading ? 'Retrying…' : 'Retry'}</button>
        </div>
      )}

      <SearchBar />
      <FilterPanel />

      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2 text-sm">
          <button
            onClick={() => setView('list')}
            className={`rounded-md px-3 py-1.5 border ${view === 'list' ? 'bg-brand-600 text-white border-brand-600' : 'border-slate-300 text-slate-700'}`}
          >List</button>
          <button
            onClick={() => setView('map')}
            className={`rounded-md px-3 py-1.5 border ${view === 'map' ? 'bg-brand-600 text-white border-brand-600' : 'border-slate-300 text-slate-700'}`}
          >Map</button>
        </div>
        <button
          onClick={() => setShowBestOnly((v) => !v)}
          className={`rounded-md px-3 py-1.5 text-sm border ${showBestOnly ? 'bg-amber-500 text-white border-amber-500' : 'border-slate-300 text-slate-700'}`}
        >{showBestOnly ? 'Show all' : 'Best match'}</button>
      </div>

      {loading && <LoadingSpinner label="Searching hospitals…" />}
      {error && <div className="rounded-md bg-rose-50 border border-rose-200 p-3 text-sm text-rose-700">{error}</div>}

      {!loading && !error && view === 'list' && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {visible.map((h) => (
            <HospitalCard key={h.id} hospital={h} service={service || undefined} isBest={h.id === bestId} />
          ))}
          {visible.length === 0 && (
            <div className="col-span-full text-center text-slate-500 py-10">No hospitals match your filters.</div>
          )}
        </div>
      )}

      {!loading && view === 'map' && (
        <Suspense fallback={<LoadingSpinner label="Loading map…" />}>
          <HospitalMap hospitals={visible} userLocation={userLocation} bestId={bestId} />
        </Suspense>
      )}
    </div>
  );
}
