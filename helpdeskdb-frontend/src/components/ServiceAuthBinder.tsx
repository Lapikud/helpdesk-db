"use client";

import { useEffect } from "react";
import { allServices } from "@/services";
import { IAccountInfo } from "@/context/AccountContext";

/**
 * Wires the account-info setter into every service exactly once.
 *
 * The services' 401 interceptor uses this callback to push a refreshed identity
 * back into React context. Previously each page did this in its render body
 * (`if (setAccountInfo) service.injectSetAccountInfo(...)`) — a side effect
 * during render, repeated ~40 times. There is only ever one setter (a stable
 * `useState` setter from the app's single AccountContext.Provider), so binding
 * it once in an effect is equivalent and correct.
 */
export default function ServiceAuthBinder({
	setAccountInfo,
}: {
	setAccountInfo: (value: IAccountInfo) => void;
}) {
	useEffect(() => {
		for (const service of allServices) {
			service.injectSetAccountInfo(setAccountInfo);
		}
	}, [setAccountInfo]);

	return null;
}
