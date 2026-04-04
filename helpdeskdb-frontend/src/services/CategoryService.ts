import { ICategory, ICategoryAdd } from "@/types/domain/DomainTypes";
import { EntityService } from "./EntityService";

export class CategoryService extends EntityService<ICategory, ICategoryAdd> {
	constructor(){
		super('categories')
	}
}
