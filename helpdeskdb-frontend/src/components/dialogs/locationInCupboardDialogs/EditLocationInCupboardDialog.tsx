import { useMemo } from "react";
import {
	ILocation,
	ILocationInCupboard,
	ILocationInCupboardWithNames,
} from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import {
	locationInCupboardFormConfig,
	locationsToOptions,
} from "../entityConfigs/locationInCupboard";

interface EditLocationInCupboardDialogProps {
	open: boolean;
	entry: ILocationInCupboardWithNames | null;
	cupboardCodeName: string;
	availableLocations: ILocation[];
	onClose: () => void;
	onConfirm: (data: ILocationInCupboard) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const EditLocationInCupboardDialog = ({
	open,
	entry,
	cupboardCodeName,
	availableLocations,
	onClose,
	onConfirm,
	isLoading,
}: EditLocationInCupboardDialogProps) => {
	const options = useMemo(
		() => ({ locations: locationsToOptions(availableLocations) }),
		[availableLocations],
	);
	const initialValues = useMemo(
		() => (entry ? { locationId: entry.locationId } : null),
		[entry],
	);

	return (
		<EntityFormDialog
			open={open}
			mode="edit"
			config={locationInCupboardFormConfig}
			initialValues={initialValues}
			options={options}
			staticValues={{ cupboardCodeName }}
			onClose={onClose}
			onConfirm={(data) =>
				onConfirm({
					id: entry!.id,
					cupboardId: entry!.cupboardId,
					locationId: data.locationId,
				})
			}
			isLoading={isLoading}
		/>
	);
};
