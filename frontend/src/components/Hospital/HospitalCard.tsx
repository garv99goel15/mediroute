import { memo } from 'react';
import { Link } from 'react-router-dom';
import type { HospitalSearchResult } from '../../types';
import { AvailabilityBadge } from './AvailabilityBadge';

interface Props {
  hospital: HospitalSearchResult;
  service?: string;
  isBest?: boolean;
}

function HospitalCardImpl({ hospital, service, isBest }: Readonly<Props>) {
  const { availability } = hospital;
  const topServices = [
    { label: 'ICU', avail: availability.icuAvailable, total: availability.icuTotal },
    { label: 'Emergency', avail: availability.emergencyAvailable, total: availability.emergencyTotal },
    { label: 'OT', avail: availability.otAvailable, total: availability.otTotal },
    { label: 'Doctors', avail: availability.doctorsAvailable, total: availability.doctorsTotal },
  ].filter((s) => s.total > 0);

  return (
    <div
      className={`rounded-xl border bg-white p-4 shadow-sm transition hover:shadow-md ${
        isBest ? 'border-brand-500 ring-2 ring-brand-200' : 'border-slate-200'
      }`}
    >
      <div className="flex items-start justify-between gap-3">
        <div>
          <div className="flex items-center gap-2">
            <h3 className="font-semibold text-slate-900">{hospital.name}</h3>
            {isBest && <span className="rounded bg-brand-600 px-2 py-0.5 text-[10px] font-bold uppercase text-white">Best match</span>}
          </div>
          <p className="text-xs text-slate-500 mt-0.5">{hospital.address}, {hospital.city}</p>
        </div>
        <div className="text-right text-xs text-slate-500">
          <div className="font-semibold text-slate-800">{hospital.distanceKm} km</div>
          <div>score {hospital.score.toFixed(2)}</div>
        </div>
      </div>

      <div className="mt-3 flex flex-wrap gap-2">
        <AvailabilityBadge availability={availability} service={service} />
        {topServices.slice(0, 3).map((s) => (
          <span key={s.label} className="rounded-full bg-slate-100 px-2 py-0.5 text-xs text-slate-700">
            {s.label} {s.avail}/{s.total}
          </span>
        ))}
      </div>

      <div className="mt-3 flex justify-end">
        <Link
          to={`/hospital/${hospital.id}`}
          className="text-sm text-brand-700 hover:text-brand-900 font-medium"
        >View details →</Link>
      </div>
    </div>
  );
}

/**
 * Memoized so SignalR patches to one hospital re-render only that card,
 * not every sibling in the results grid.
 */
export const HospitalCard = memo(HospitalCardImpl, (prev, next) =>
  prev.hospital === next.hospital
  && prev.service === next.service
  && prev.isBest === next.isBest
);
