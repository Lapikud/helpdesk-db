"use client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { useState } from "react";
import { ApiError } from "@/services/errors";

export default function Providers({ children }: { children: React.ReactNode }) {
	// Lazy initializer, not `new QueryClient()` inline: the inline form would
	// build a fresh client on every render, throwing the cache away each time.
	const [queryClient] = useState(
		() =>
			new QueryClient({
				defaultOptions: {
					queries: {
						staleTime: 30_000,
						gcTime: 5 * 60_000,
						// Matches the previous useEffect behavior (fetch on
						// mount only). Opt in per-query where it's useful.
						refetchOnWindowFocus: false,
						retry: (failureCount, error) => {
							// 4xx won't succeed on retry, and 401 is already
							// handled by the refresh interceptor in
							// BaseService — retrying would fight it.
							if (
								error instanceof ApiError &&
								error.statusCode >= 400 &&
								error.statusCode < 500
							) {
								return false;
							}
							return failureCount < 2;
						},
					},
					mutations: {
						retry: false,
					},
				},
			}),
	);

	return (
		<QueryClientProvider client={queryClient}>
			{children}
			<ReactQueryDevtools initialIsOpen={false} />
		</QueryClientProvider>
	);
}
