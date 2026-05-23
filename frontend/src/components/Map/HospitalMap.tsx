import { MapContainer, TileLayer, Marker, Popup, useMap } from 'react-leaflet';
import L from 'leaflet';
import { useEffect } from 'react';
import type { HospitalSearchResult } from '../../types';
import { Link } from 'react-router-dom';

// Default icon fix — Leaflet's default markers break with bundlers.
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
});

const hospitalIcon = (color: string) =>
  L.divIcon({
    className: 'custom-marker',
    html: `<div style="background:${color};width:18px;height:18px;border-radius:50%;border:3px solid white;box-shadow:0 0 0 2px ${color}80;"></div>`,
    iconSize: [18, 18],
    iconAnchor: [9, 9],
  });

function FitBounds({ points }: { points: [number, number][] }) {
  const map = useMap();
  useEffect(() => {
    if (points.length === 0) return;
    if (points.length === 1) { map.setView(points[0], 13); return; }
    map.fitBounds(points, { padding: [40, 40] });
  }, [points, map]);
  return null;
}

interface Props {
  hospitals: HospitalSearchResult[];
  userLocation: { lat: number; lng: number } | null;
  bestId?: number | null;
}

export function HospitalMap({ hospitals, userLocation, bestId }: Props) {
  const center: [number, number] = userLocation
    ? [userLocation.lat, userLocation.lng]
    : [28.6139, 77.2090];

  const points: [number, number][] = hospitals.map((h) => [h.lat, h.lng]);
  if (userLocation) points.push([userLocation.lat, userLocation.lng]);

  return (
    <div className="h-[500px] w-full rounded-xl overflow-hidden border border-slate-200">
      <MapContainer center={center} zoom={11} className="h-full w-full">
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />
        <FitBounds points={points} />
        {userLocation && (
          <Marker
            position={[userLocation.lat, userLocation.lng]}
            icon={hospitalIcon('#2563eb')}
          >
            <Popup>You are here</Popup>
          </Marker>
        )}
        {hospitals.map((h) => {
          const ratio = (h.availability.icuAvailable + h.availability.generalAvailable
            + h.availability.emergencyAvailable + h.availability.otAvailable)
            / Math.max(1, h.availability.icuTotal + h.availability.generalTotal
              + h.availability.emergencyTotal + h.availability.otTotal);
          const color = h.id === bestId ? '#1d4ed8' : (ratio > 0.5 ? '#10b981' : ratio >= 0.2 ? '#f59e0b' : '#ef4444');
          return (
            <Marker key={h.id} position={[h.lat, h.lng]} icon={hospitalIcon(color)}>
              <Popup>
                <div className="text-sm">
                  <div className="font-semibold">{h.name}</div>
                  <div className="text-slate-600">{h.distanceKm} km away · score {h.score.toFixed(2)}</div>
                  <Link to={`/hospital/${h.id}`} className="mt-1 inline-block text-brand-700 font-medium">View details →</Link>
                </div>
              </Popup>
            </Marker>
          );
        })}
      </MapContainer>
    </div>
  );
}
