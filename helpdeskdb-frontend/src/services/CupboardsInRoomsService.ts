import { ICupboardInRoom, ICupboardInRoomAdd } from "@/types/domain/DomainTypes";
import { EntityService } from "./EntityService";

export class CupboardsInRoomsService extends EntityService<ICupboardInRoom, ICupboardInRoomAdd> {
	constructor(){
		super('cupboardsinrooms')
	}
}
