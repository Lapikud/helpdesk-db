import { ILocationInCupboard, ILocationInCupboardAdd } from "@/types/domain/DomainTypes";
import { EntityService } from "./EntityService";

export class LocationInCupboardService extends EntityService<ILocationInCupboard, ILocationInCupboardAdd> {
	constructor() {
		super('locationsincupboards')
	}

}
