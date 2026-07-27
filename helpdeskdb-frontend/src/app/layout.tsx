"use client";
import "./globals.css";
import Header from "@/components/Header";
import Footer from "@/components/Footer";
import { AccountContext, IAccountInfo } from "@/context/AccountContext";
import { useEffect, useState } from "react";
import Spinner from "@/components/LoadingSpinner";
import AuthGuard from "@/components/AuthGuard";
import { accountService } from "@/services";
import Providers from "./providers";
import ServiceAuthBinder from "@/components/ServiceAuthBinder";
import QueryCacheReset from "@/components/QueryCacheReset";
import "../../i18n";

export default function RootLayout({
	children,
}: Readonly<{
	children: React.ReactNode;
}>) {
	const [accountInfo, setAccountInfo] = useState<IAccountInfo | undefined>();
	const [hydrated, setHydrated] = useState(false);

	useEffect(() => {
		(async () => {
			const result = await accountService.meAsync();
			if (result.data) {
				setAccountInfo({
					id: result.data.id,
					name: result.data.username,
					roles: result.data.roles,
				});
			} else {
				setAccountInfo({});
			}
			setHydrated(true);
		})();
	}, []);

	return (
		<html lang="en">
			<body>
				{/* Rendered unconditionally and above the hydration gate:
				    child effects run before the parent's, so the services are
				    wired before the /me call below fires. */}
				<ServiceAuthBinder setAccountInfo={setAccountInfo} />
				<Providers>
					{!hydrated ? (<Spinner className="h-64" />) : (
						<AccountContext.Provider
							value={{
								accountInfo: accountInfo,
								setAccountInfo: setAccountInfo,
							}}
						>
							{/* Inside both Providers and AccountContext: it needs
						    the query client and the identity to compare. */}
						<QueryCacheReset />
						<Header />
							<div className="px-3 sm:px-4">
								<main role="main" className="w-full text-center">
									<AuthGuard>{children}</AuthGuard>
								</main>
							</div>
							<Footer />
						</AccountContext.Provider>
					)}
				</Providers>
			</body>
		</html>
	);
}
