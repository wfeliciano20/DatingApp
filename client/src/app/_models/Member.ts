import { Photo } from './Photo';
export interface Member {
  id: number;
  userName: string;
  photoUrl: string;
  age: number;
  knownAs: string;
  created: Date;
  lastActive: Date;
  gender?: any;
  introduction: string;
  lookingFor: string;
  interest?: any;
  city: string;
  country: string;
  photos: Photo[];
}
