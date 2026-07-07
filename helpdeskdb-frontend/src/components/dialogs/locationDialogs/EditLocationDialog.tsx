import { useMemo } from "react";
import { ILocation } from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import { locationFormConfig, locationToForm } from "../entityConfigs/location";

interface EditLocationDialogProps {
	open: boolean;
	location: ILocation | null;
	onClose: () => void;
	onConfirm: (data: ILocation) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const EditLocationDialog = ({
	open,
	location,
	onClose,
	onConfirm,
	isLoading,
}: EditLocationDialogProps) => {
	const initialValues = useMemo(
		() => (location ? locationToForm(location) : null),
		[location],
	);

	return (
		<EntityFormDialog
			open={open}
			mode="edit"
			config={locationFormConfig}
			initialValues={initialValues}
			onClose={onClose}
			onConfirm={(data) => onConfirm({ id: location!.id, ...data })}
			isLoading={isLoading}
		/>
	);
};
