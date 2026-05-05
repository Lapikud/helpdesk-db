"use client";

import { createContext } from "react";

export interface IAccountInfo {
	roles?: string[];
	name?: string;
	id?: string;
}

export interface IAccountState {
	accountInfo?: IAccountInfo;
	setAccountInfo?: (value: IAccountInfo) => void;
}

export const AccountContext = createContext<IAccountState>({});
