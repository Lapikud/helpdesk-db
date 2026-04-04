import { ILocation, ILocationAdd } from "@/types/domain/DomainTypes";
import { EntityService } from "./EntityService";

export class LocationService extends EntityService<ILocation, ILocationAdd> {
	constructor(){
		super('locations')
	}
}
