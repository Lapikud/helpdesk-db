"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { CategoryService } from "@/services/CategoryService";
import Link from "next/link";
import { useContext, useEffect, useMemo, useState } from "react";
import { ICategory } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import {
	ActionCell,
	EditButton,
	DeleteButton,
} from "@/components/TableActions";

export default function Categories() {
	const { t: tCategory } = useTranslation("category");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const categoryService = useMemo(() => new CategoryService(), []);
	if (setAccountInfo) categoryService.injectSetAccountInfo(setAccountInfo);

	const [data, setData] = useState<ICategory[]>([]);
	const [hydrated, setHydrated] = useState(false);
	const isAdmin = accountInfo?.roles?.includes("admins");

	useEffect(() => {
		setHydrated(true);
	}, []);

	useEffect(() => {
		if (!hydrated) return;
		categoryService.getAllAsync().then((result) => {
			if (!result.errors) setData(result.data!);
		});
	}, [hydrated, categoryService]);

	if (!hydrated) return <Spinner className="h-64" />;

	const columns = isAdmin
		? [tCategory("CategoryName"), tCommon("Comment"), tCommon("Actions")]
		: [tCategory("CategoryName"), tCommon("Comment")];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.categoryName,
			item.comment || "-",
			...(isAdmin
				? [
						<ActionCell key="actions">
							<EditButton
								href={`/categories/edit/${item.id}`}
								label={tCommon("EditLink")}
							/>
							<DeleteButton
								href={`/categories/delete/${item.id}`}
								label={tCommon("DeleteLink")}
							/>
						</ActionCell>,
					]
				: []),
		],
	}));

	return (
		<ListPageWrapper
			title={tCategory("Categories")}
			createButton={
				isAdmin && (
					<Link
						href="/categories/create"
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
