import { useHospitalStore } from '../../store/hospitalStore';
import { useHospitalSearch } from '../../hooks/useHospitalSearch';

const services: { value: string; label: string }[] = [
  { value: '', label: 'Any service' },
  { value: 'ICU', label: 'ICU' },
  { value: 'Emergency', label: 'Emergency' },
  { value: 'OT', label: 'Operation Theatre' },
  { value: 'Maternity', label: 'Maternity' },
  { value: 'General', label: 'General Ward' },
  { value: 'Doctor', label: 'Doctor consultation' },
];

export function SearchBar() {
  const { service, setService, radius, setRadius } = useHospitalStore();
  const { search } = useHospitalSearch();

  return (
    <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-[minmax(0,260px)_120px_1fr] sm:items-end">
        <div>
          <label className="block text-xs font-medium text-slate-600 mb-1">Service</label>
          <div className="relative">
            <select
              value={service}
              onChange={(e) => setService(e.target.value as any)}
              className="w-full appearance-none rounded-md border border-slate-300 bg-white px-3 py-2 pr-9 text-sm text-slate-900 shadow-sm focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500"
            >
              {services.map((s) => <option key={s.value} value={s.value}>{s.label}</option>)}
            </select>
            <svg
              aria-hidden="true"
              viewBox="0 0 20 20"
              className="pointer-events-none absolute right-2.5 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400"
              fill="none" stroke="currentColor" strokeWidth="2"
            >
              <path strokeLinecap="round" strokeLinejoin="round" d="M6 8l4 4 4-4" />
            </svg>
          </div>
        </div>
        <div>
          <label className="block text-xs font-medium text-slate-600 mb-1">Radius (km)</label>
          <input
            type="number"
            min={1}
            max={100}
            value={radius}
            onChange={(e) => setRadius(Number(e.target.value))}
            className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm shadow-sm focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500"
          />
        </div>
        <button
          onClick={() => search()}
          className="h-10 rounded-md bg-brand-600 px-4 text-sm font-medium text-white shadow-sm hover:bg-brand-700 sm:justify-self-end sm:w-32"
        >Search</button>
      </div>
    </div>
  );
}
