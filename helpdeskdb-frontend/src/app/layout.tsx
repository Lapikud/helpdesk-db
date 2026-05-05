"use client";
import "./globals.css";
import Header from "@/components/Header";
import Footer from "@/components/Footer";
import { AccountContext, IAccountInfo } from "@/context/AccountContext";
import { useEffect, useState } from "react";
import Spinner from "@/components/LoadingSpinner";
import AuthGuard from "@/components/AuthGuard";
import { AccountService } from "@/services/AccountService";
import "../../i18n";

export default function RootLayout({
	children,
}: Readonly<{
	children: React.ReactNode;
}>) {
	const [accountInfo, setAccountInfo] = useState<IAccountInfo | undefined>();
	const [hydrated, setHydrated] = useState(false);

	useEffect(() => {
		const accountService = new AccountService();
		(async () => {
			const result = await accountService.meAsync();
			if (result.data) {
				setAccountInfo({
					id: result.data.id,
					name: result.data.username,
					roles: result.data.roles,
				});
				console.log("result:", result.data);
			} else {
				console.log("result.errors:", result.errors, "status:", result.statusCode);
				setAccountInfo({});
			}
			setHydrated(true);
		})();
	}, []);

	return (
		<html lang="en">
			<body>
				{!hydrated ? (<Spinner className="h-64" />) : (
					<AccountContext.Provider
						value={{
							accountInfo: accountInfo,
							setAccountInfo: setAccountInfo,
						}}
					>
						<Header />
						<div className="px-3 sm:px-4">
							<main role="main" className="w-full text-center">
								<AuthGuard>{children}</AuthGuard>
							</main>
						</div>
						<Footer />
					</AccountContext.Provider>
				)}
			</body>
		</html>
	);
}
