import { SocialNetwork } from './SocialNetwork';
import { Event } from './Event';

export interface Speaker {
  id: number;
  name: string;
  shortBio: string;
  imageUrl: string;
  phone: string;
  email: string;
  socialNetworks: SocialNetwork[];
  events: Event[];
}

