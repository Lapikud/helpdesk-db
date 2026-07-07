import { ILocationAdd } from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import { locationFormConfig } from "../entityConfigs/location";

interface CreateLocationDialogProps {
	open: boolean;
	onClose: () => void;
	onConfirm: (data: ILocationAdd) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const CreateLocationDialog = ({
	open,
	onClose,
	onConfirm,
	isLoading,
}: CreateLocationDialogProps) => (
	<EntityFormDialog
		open={open}
		mode="create"
		config={locationFormConfig}
		onClose={onClose}
		onConfirm={onConfirm}
		isLoading={isLoading}
	/>
);
