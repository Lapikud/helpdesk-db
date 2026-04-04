import { IRole, IRoleAdd } from "@/types/domain/DomainTypes";
import { EntityService } from "./EntityService";

export class RoleService extends EntityService<IRole, IRoleAdd> {
	constructor(){
		super('roles')
	}
}
