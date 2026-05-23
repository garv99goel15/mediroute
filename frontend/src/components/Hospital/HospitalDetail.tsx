import type { Availability, Hospital } from '../../types';
import { StatusBadge } from '../shared/StatusBadge';

interface Props {
  hospital: Hospital;
  availability: Availability;
}

function colorFor(a: number, t: number): 'green' | 'yellow' | 'red' | 'grey' {
  if (t <= 0) return 'grey';
  const r = a / t;
  if (r > 0.5) return 'green';
  if (r >= 0.2) return 'yellow';
  return 'red';
}

export function HospitalDetail({ hospital, availability }: Props) {
  return (
    <div className="space-y-6">
      <header className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
        <h1 className="text-2xl font-bold text-slate-900">{hospital.name}</h1>
        <p className="text-slate-600 mt-1">{hospital.address}, {hospital.city}, {hospital.state}</p>
        <p className="text-slate-500 text-sm mt-1">📞 {hospital.phone} · ✉ {hospital.email}</p>
      </header>

      <section>
        <h2 className="font-semibold text-slate-800 mb-3">Live availability</h2>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          {availability.departments.map((d) => (
            <div key={d.departmentId} className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
              <div className="text-xs uppercase tracking-wide text-slate-500">{d.type}</div>
              <div className="mt-1 font-semibold text-slate-800">{d.name}</div>
              <div className="mt-2"><StatusBadge color={d.colorCode} label={`${d.available} / ${d.total} beds`} /></div>
            </div>
          ))}
          <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
            <div className="text-xs uppercase tracking-wide text-slate-500">Doctors</div>
            <div className="mt-1 font-semibold text-slate-800">Available now</div>
            <div className="mt-2"><StatusBadge color={colorFor(availability.doctorsAvailable, availability.doctorsTotal)} label={`${availability.doctorsAvailable} / ${availability.doctorsTotal}`} /></div>
          </div>
          <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
            <div className="text-xs uppercase tracking-wide text-slate-500">Ambulances</div>
            <div className="mt-1 font-semibold text-slate-800">Ready</div>
            <div className="mt-2"><StatusBadge color={colorFor(availability.ambulancesAvailable, availability.ambulancesTotal)} label={`${availability.ambulancesAvailable} / ${availability.ambulancesTotal}`} /></div>
          </div>
        </div>
      </section>

      <section>
        <h2 className="font-semibold text-slate-800 mb-3">Doctors</h2>
        <div className="overflow-x-auto rounded-xl border border-slate-200 bg-white">
          <table className="w-full text-sm">
            <thead className="bg-slate-50 text-slate-600">
              <tr>
                <th className="px-4 py-2 text-left">Name</th>
                <th className="px-4 py-2 text-left">Specialization</th>
                <th className="px-4 py-2 text-left">Experience</th>
                <th className="px-4 py-2 text-left">Fee</th>
                <th className="px-4 py-2 text-left">Status</th>
              </tr>
            </thead>
            <tbody>
              {hospital.doctors.map((d) => (
                <tr key={d.id} className="border-t border-slate-100">
                  <td className="px-4 py-2 font-medium text-slate-800">{d.name}</td>
                  <td className="px-4 py-2 text-slate-600">{d.specialization}</td>
                  <td className="px-4 py-2 text-slate-600">{d.yearsOfExperience} yrs</td>
                  <td className="px-4 py-2 text-slate-600">₹{d.consultationFee}</td>
                  <td className="px-4 py-2">
                    <StatusBadge color={d.isAvailable ? 'green' : 'red'} label={d.isAvailable ? 'Available' : 'Unavailable'} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  );
}
