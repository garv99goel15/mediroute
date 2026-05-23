export type DepartmentType = 'ICU' | 'General' | 'Emergency' | 'OT' | 'Maternity';
export type BedStatus = 'Available' | 'Occupied' | 'Maintenance' | 'Reserved';
export type AmbulanceStatus = 'Available' | 'Dispatched' | 'Maintenance';
export type UserRole = 'SuperAdmin' | 'HospitalAdmin';

export interface DepartmentAvailability {
  departmentId: number;
  name: string;
  type: DepartmentType;
  available: number;
  total: number;
  colorCode: 'green' | 'yellow' | 'red' | 'grey';
}

export interface Availability {
  hospitalId: number;
  timestamp: string;
  icuAvailable: number;
  icuTotal: number;
  generalAvailable: number;
  generalTotal: number;
  emergencyAvailable: number;
  emergencyTotal: number;
  otAvailable: number;
  otTotal: number;
  maternityAvailable: number;
  maternityTotal: number;
  doctorsAvailable: number;
  doctorsTotal: number;
  ambulancesAvailable: number;
  ambulancesTotal: number;
  departments: DepartmentAvailability[];
}

export interface HospitalSearchResult {
  id: number;
  name: string;
  address: string;
  city: string;
  lat: number;
  lng: number;
  distanceKm: number;
  score: number;
  availability: Availability;
}

export interface Hospital {
  id: number;
  name: string;
  address: string;
  city: string;
  state: string;
  lat: number;
  long: number;
  phone: string;
  email: string;
  isActive: boolean;
  departments: { id: number; name: string; type: DepartmentType; totalBeds: number; isActive: boolean; beds: Bed[] }[];
  doctors: Doctor[];
  ambulances: { id: number; vehicleNumber: string; status: AmbulanceStatus; lastUpdatedAt: string }[];
}

export interface Doctor {
  id: number;
  name: string;
  specialization: string;
  isAvailable: boolean;
  nextAvailableAt: string | null;
  consultationFee: number;
  yearsOfExperience: number;
}

export interface Bed {
  id: number;
  bedNumber: string;
  status: BedStatus;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  username: string;
  role: UserRole;
  hospitalId: number | null;
}

export interface AdminDashboard {
  hospitalId: number;
  hospitalName: string;
  totalBeds: number;
  availableBeds: number;
  occupiedBeds: number;
  maintenanceBeds: number;
  doctorsTotal: number;
  doctorsAvailable: number;
  ambulancesTotal: number;
  ambulancesAvailable: number;
  availability: Availability;
  recentActivity: RecentActivity[];
}

export interface RecentActivity {
  resourceType: string;
  description: string;
  timestamp: string;
  updatedBy: string | null;
}

export interface AvailabilityUpdateEvent {
  hospitalId: number;
  updatedResourceType?: string;
  availability: Availability;
  timestamp: string;
}

export type ServiceFilter = '' | 'ICU' | 'General' | 'Emergency' | 'OT' | 'Maternity' | 'Doctor';
