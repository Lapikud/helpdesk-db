"use client";

import { useContext, useEffect, useRef } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { AccountContext } from "@/context/AccountContext";

/**
 * Drops the whole query cache whenever the signed-in identity changes.
 *
 * Without this the cache outlives the session: log out as an admin and back in
 * as a member, and the second user renders the first user's cached lists until
 * each query happens to go stale.
 *
 * Two details make this an effect on `accountInfo.id` rather than a
 * `queryClient.clear()` call inside the logout handler:
 *
 *  - Timing. Clearing inside `handleLogout` runs while the current page is
 *    still mounted, so every active query immediately refetches — with the
 *    auth cookies already deleted. That fires a burst of 401s the interceptor
 *    then tries, and fails, to refresh. Effects run after commit, by which
 *    point AuthGuard has already swapped the page for a spinner and no
 *    observers are left to refetch.
 *
 *  - Keying on `id`, not on the `accountInfo` object. BaseService's 401
 *    interceptor calls `setAccountInfo` with the *same* identity after a
 *    successful token refresh; comparing object identity would wipe the cache
 *    on every routine refresh.
 *
 * Covers every transition in one place: login, logout, switching users, and
 * the interceptor's forced `setAccountInfo({})` when a refresh fails.
 */
export default function QueryCacheReset() {
	const { accountInfo } = useContext(AccountContext);
	const queryClient = useQueryClient();
	const previousId = useRef<string | undefined>(undefined);

	useEffect(() => {
		const id = accountInfo?.id;
		if (previousId.current !== id) {
			queryClient.clear();
			previousId.current = id;
		}
	}, [accountInfo?.id, queryClient]);

	return null;
}
