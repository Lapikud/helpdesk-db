"use client";

import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { qk } from "@/lib/queryKeys";
import { unwrap } from "@/services/errors";
import { overviewService } from "@/services";

/**
 * The overview page's aggregated asset list.
 *
 * `searchTerm` is part of the key because the server filters on it — each term
 * is a distinct response and needs its own cache entry.
 *
 * `keepPreviousData` reproduces the behaviour the page used to hand-roll with a
 * `hasLoadedOnce` ref: while a new search term loads, the previous list stays
 * on screen instead of flashing a full-page spinner. `isLoading` is therefore
 * true only for the very first fetch; use `isFetching` if a subtler in-flight
 * indicator is ever needed.
 */
export const useOverview = (searchTerm: string) =>
	useQuery({
		queryKey: qk.overview(searchTerm),
		queryFn: () => unwrap(overviewService.getOverview(searchTerm)),
		placeholderData: keepPreviousData,
	});
