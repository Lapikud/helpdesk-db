import { useContext, useMemo } from "react";
import { IAsset, IRemovedAssetAdd } from "@/types/domain/DomainTypes";
import { AccountContext } from "@/context/AccountContext";
import { EntityFormDialog } from "../common/EntityFormDialog";
import {
	assetsToOptions,
	removedAssetCreateConfig,
} from "../entityConfigs/removedAsset";

interface CreateRemovedAssetDialogProps {
	open: boolean;
	assets: IAsset[];
	onClose: () => void;
	onConfirm: (data: IRemovedAssetAdd) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const CreateRemovedAssetDialog = ({
	open,
	assets,
	onClose,
	onConfirm,
	isLoading,
}: CreateRemovedAssetDialogProps) => {
	const { accountInfo } = useContext(AccountContext);
	const options = useMemo(() => ({ assets: assetsToOptions(assets) }), [
		assets,
	]);

	return (
		<EntityFormDialog
			open={open}
			mode="create"
			config={removedAssetCreateConfig}
			options={options}
			staticValues={{ userName: accountInfo?.name ?? "" }}
			onClose={onClose}
			onConfirm={(data) =>
				onConfirm({
					assetId: data.assetId,
					comment: data.comment,
				})
			}
			isLoading={isLoading}
		/>
	);
};
