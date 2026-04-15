"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { OwnerService } from "@/services/OwnerService";
import Link from "next/link";
import { useContext, useEffect, useMemo, useState } from "react";
import { IOwner } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import {
	ActionCell,
	EditButton,
	DeleteButton,
} from "@/components/TableActions";

export default function Owners() {
	const { t: tOwner } = useTranslation("owner");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const ownerService = useMemo(() => new OwnerService(), []);
	if (setAccountInfo) ownerService.injectSetAccountInfo(setAccountInfo);

	const [data, setData] = useState<IOwner[]>([]);
	const [hydrated, setHydrated] = useState(false);
	const isAdmin = accountInfo?.roles?.includes("admins");

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;
		ownerService.getAllAsync().then((result) => {
			if (!result.errors) setData(result.data!);
		});
	}, [hydrated, ownerService]);

	if (!hydrated) return <Spinner className="h-64" />;

	const columns = isAdmin
		? [tOwner("OwnerName"), tCommon("Comment"), tCommon("Actions")]
		: [tOwner("OwnerName"), tCommon("Comment")];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.ownerName,
			item.comment || "-",
			...(isAdmin
				? [
						<ActionCell key="actions">
							<EditButton
								href={`/owners/edit/${item.id}`}
								label={tCommon("EditLink")}
							/>
							<DeleteButton
								href={`/owners/delete/${item.id}`}
								label={tCommon("DeleteLink")}
							/>
						</ActionCell>,
					]
				: []),
		],
	}));

	return (
		<ListPageWrapper
			title={tOwner("Owners")}
			createButton={
				isAdmin && (
					<Link
						href="/owners/create"
						className="bg-[#ff9800] hover:bg-[#f0941d] text-white font-medium px-6 py-3 rounded-full text-sm whitespace-nowrap transition-colors duration-150"
					>
						{tCommon("CreateNewLink")}
					</Link>
				)
			}
		>
			<DataTable columns={columns} rows={rows} />
		</ListPageWrapper>
	);
}
