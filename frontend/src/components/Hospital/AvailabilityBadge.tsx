import { StatusBadge } from '../shared/StatusBadge';
import type { Availability } from '../../types';

interface Props {
  availability: Availability;
  service?: string;
}

export function AvailabilityBadge({ availability, service }: Props) {
  const colorFor = (a: number, t: number) => {
    if (t <= 0) return 'grey' as const;
    const r = a / t;
    if (r > 0.5) return 'green' as const;
    if (r >= 0.2) return 'yellow' as const;
    return 'red' as const;
  };

  if (service) {
    const map: Record<string, [number, number]> = {
      ICU: [availability.icuAvailable, availability.icuTotal],
      General: [availability.generalAvailable, availability.generalTotal],
      Emergency: [availability.emergencyAvailable, availability.emergencyTotal],
      OT: [availability.otAvailable, availability.otTotal],
      Maternity: [availability.maternityAvailable, availability.maternityTotal],
      Doctor: [availability.doctorsAvailable, availability.doctorsTotal],
    };
    const [avail, total] = map[service] ?? [0, 0];
    return <StatusBadge color={colorFor(avail, total)} label={`${service}: ${avail}/${total}`} />;
  }

  const total = availability.icuTotal + availability.generalTotal + availability.emergencyTotal + availability.otTotal + availability.maternityTotal;
  const avail = availability.icuAvailable + availability.generalAvailable + availability.emergencyAvailable + availability.otAvailable + availability.maternityAvailable;
  return <StatusBadge color={colorFor(avail, total)} label={`Beds: ${avail}/${total}`} />;
}
