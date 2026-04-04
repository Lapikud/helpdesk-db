import { IUserRole, IUserRoleAdd } from "@/types/domain/DomainTypes";
import { EntityService } from "./EntityService";

export class UserRoleService extends EntityService<IUserRole, IUserRoleAdd> {
	constructor(){
		super('userRoles')
	}
}
