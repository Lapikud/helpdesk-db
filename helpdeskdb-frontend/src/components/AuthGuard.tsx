"use client";

import { useContext, useEffect } from "react";
import { usePathname, useRouter } from "next/navigation";
import { AccountContext } from "@/context/AccountContext";
import Spinner from "@/components/LoadingSpinner";

export default function AuthGuard({ children }: { children: React.ReactNode }) {
	const { accountInfo } = useContext(AccountContext);
	const router = useRouter();
	const pathname = usePathname();
	const isPublic = pathname.includes("/login");

	useEffect(() => {
		if (isPublic) return;
		if (accountInfo === undefined) return; // still hydrating
		if (!accountInfo.jwt) {
			router.push("/login");
		}
	}, [accountInfo, router, isPublic]);

	console.log(isPublic);
	if (isPublic) return <>{children}</>;
	if (accountInfo === undefined || !accountInfo.jwt) return <Spinner className="h-64" />;

	return <>{children}</>;
}
