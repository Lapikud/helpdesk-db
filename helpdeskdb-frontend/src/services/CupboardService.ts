import { ICupboard, ICupboardAdd } from "@/types/domain/DomainTypes";
import { EntityService } from "./EntityService";

export class CupboardService extends EntityService<ICupboard, ICupboardAdd> {
	constructor(){
		super('cupboards')
	}
}
