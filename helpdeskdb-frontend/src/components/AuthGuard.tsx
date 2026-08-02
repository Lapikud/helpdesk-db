"use client";

import { useEffect } from "react";
import { usePathname, useRouter } from "next/navigation";
import { usePermissions } from "@/hooks/usePermissions";
import Spinner from "@/components/LoadingSpinner";

// First path segments that require canManage (admins/helpdesk_db_admins).
// removedAssets, owners, and assetReservations are deliberately absent —
// they stay viewable by all logged-in users.
const ADMIN_ROUTES = [
	"dbassets",
	"categoryAssets",
	"locationAssets",
	"ownerAssets",
	"locations",
	"users",
	"roles",
	"refreshTokens",
	"cupboardsInRooms",
	"rooms",
	"cupboards",
];

export default function AuthGuard({ children }: { children: React.ReactNode }) {
	const { accountInfo, canManage } = usePermissions();
	const router = useRouter();
	const pathname = usePathname();
	const isPublic = pathname.includes("/login");
	// trailingSlash is on — match the first segment, not the raw pathname.
	// Case-insensitive: on case-insensitive filesystems Next serves routes
	// under any casing (e.g. /CupboardsInRooms), which must not skip the gate.
	const segment = pathname.split("/")[1]?.toLowerCase();
	const needsAdmin = ADMIN_ROUTES.some((r) => r.toLowerCase() === segment);

	useEffect(() => {
		if (isPublic) return;
		if (accountInfo === undefined) return; // still hydrating
		if (!accountInfo.id) {
			router.push("/login");
		} else if (needsAdmin && !canManage) {
			router.push("/");
		}
	}, [accountInfo, router, isPublic, needsAdmin, canManage]);

	if (isPublic) return <>{children}</>;
	if (accountInfo === undefined || !accountInfo.id) return <Spinner className="h-64" />;
	if (needsAdmin && !canManage) return <Spinner className="h-64" />;

	return <>{children}</>;
}
