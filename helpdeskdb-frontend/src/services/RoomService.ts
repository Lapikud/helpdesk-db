import { IRoom, IRoomAdd } from "@/types/domain/DomainTypes";
import { EntityService } from "./EntityService";

export class RoomService extends EntityService<IRoom, IRoomAdd> {
	constructor(){
		super('rooms')
	}
}
