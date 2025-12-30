import { Batch } from './Batch';
import { SocialNetwork } from './SocialNetwork';
import { Speaker } from './Speaker';

export class Event {

    constructor() { }

    id: number;
    location: string;
    eventDate: Date;
    theme: string;
    peopleCount: number;
    imageUrl: string;
    phone: string;
    email: string;
    batches: Batch[];
    socialNetworks: SocialNetwork[];
    speakerEvents: Speaker[];
}

