import { IDomainId } from "../IDomainId";

// Asset
export interface IAsset extends IDomainId {
	assetName: string;
	comment: string;
	serialNumber?: string | null;
	barcode?: string | null;
}

export interface IAssetAdd {
	assetName: string;
	comment: string;
	serialNumber?: string | null;
	barcode?: string | null;
}

// Asset reservation
export interface IAssetReservation extends IDomainId {
	userId: string;
	assetId: string;
	reservationFrom: Date;
	reservationTo: Date;
	isReturned: boolean;
}

export interface IAssetReservationAdd {
	userId: string;
	assetId: string;
	reservationFrom: string;
	reservationTo: string;
}

export interface IAssetReservationUpdate {
	assetReservationId: string;
	userId: string;
	assetId: string;
	reservationFrom: Date;
	reservationTo: Date;
}

export interface IAssetReservationWithNames extends IAssetReservation {
	assetName: string;
	userName: string;
	isRemoved?: boolean;
}

// Category
export interface ICategory extends IDomainId {
	categoryName: string;
	comment: string;
}

export interface ICategoryAdd {
	categoryName: string;
	comment: string;
}

// Category asset
export interface ICategoryAsset extends IDomainId {
	comment: string;
	categoryId: string;
	assetId: string;
	createdBy: string;
}

export interface ICategoryAssetWithNames extends ICategoryAsset {
	assetName: string;
	categoryName: string;
}

export interface ICategoryAssetAdd {
	comment: string;
	categoryId: string;
	assetId: string;
	createdBy: string;
}

// Cupboard
export interface ICupboard extends IDomainId {
	codeName: string;
}

export interface ICupboardAdd {
	codeName: string;
}

// Cupboard in room
export interface ICupboardInRoom extends IDomainId {
	comment: string;
	cupboardId: string;
	roomId: string;
}

export interface ICupboardInRoomWithNames extends ICupboardInRoom {
	codeName: string;
	roomName: string;
}

export interface ICupboardInRoomAdd {
	comment: string;
	cupboardId: string;
	roomId: string;
}

// Location
export interface ILocation extends IDomainId {
	locationName: string;
	shelfNum: number;
	column: number;
}

export interface ILocationAdd {
	locationName: string;
	shelfNum: number;
	column: number;
}

// Location asset
export interface ILocationAsset extends IDomainId {
	locationId: string;
	assetId: string;
	createdBy: string;
}

export interface ILocationAssetWithNames extends ILocationAsset {
	assetName: string;
	locationName: string;
}

export interface ILocationAssetAdd {
	locationId: string;
	assetId: string;
	createdBy: string;
}

// Location in cupboard
export interface ILocationInCupboard extends IDomainId {
	locationId: string;
	cupboardId: string;
}

export interface ILocationInCupboardWithNames extends ILocationInCupboard {
	codeName: string;
	locationName: string;
}

export interface ILocationInCupboardAdd {
	locationId: string;
	cupboardId: string;
}

// Owner
export interface IOwner extends IDomainId {
	ownerName: string;
	comment: string;
}

export interface IOwnerAdd {
	ownerName: string;
	comment: string;
}

// Owner asset
export interface IOwnerAsset extends IDomainId {
	ownerId: string;
	assetId: string;
	createdBy: string;
}

export interface IOwnerAssetAdd {
	ownerId: string;
	assetId: string;
	createdBy: string;
}

// Removed asset
export interface IRemovedAsset extends IDomainId {
	assetId: string;
	comment: string;
	removedBy: string;
}

export interface IRemovedAssetAdd {
	assetId: string;
	comment: string;
}

export interface IRemovedAssetWithAssetName extends IRemovedAsset {
	assetName: string;
}

// Room
export interface IRoom extends IDomainId {
	roomName: string;
	comment: string;
}

export interface IRoomAdd {
	roomName: string;
	comment: string;
}

// User asset
export interface IUserAsset extends IDomainId {
	comment: string;
	userId: string;
	assetId: string;
	createdBy: string;
}

export interface IUserAssetWithNames extends IUserAsset {
	assetName: string;
	userName: string;
}

export interface IUserAssetAdd {
	userId: string;
	assetId: string;
}

// User
export interface IUser extends IDomainId {
	username: string;
}

// Role
export interface IRole extends IDomainId {
	name: string;
}

export interface IRoleAdd {
	name: string;
}

// User role
export interface IUserRole extends IDomainId {
	roleId: string;
	userId: string;
}

export interface IUserRoleAdd {
	roleId: string;
	userId: string;
}

export interface IUserRoleWithUsernameAndRoleName extends IUserRole {
	username: string;
	rolename: string;
}

// Refresh token
export interface IRefreshToken extends IDomainId {
	userId: string;
	refreshToken: string;
	previousRefreshToken: string;
	expiration: Date;
	previousExpiration: Date;
}

export interface IRefreshTokenWithUsername extends IRefreshToken {
	username: string;
}
