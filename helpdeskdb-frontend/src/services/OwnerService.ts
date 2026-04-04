import { IOwner, IOwnerAdd } from "@/types/domain/DomainTypes";
import { EntityService } from "./EntityService";

export class OwnerService extends EntityService<IOwner, IOwnerAdd> {
	constructor(){
		super('owners')
	}
}
