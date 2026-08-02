import { useMemo } from "react";
import { FieldValues } from "react-hook-form";
import { IDomainId } from "@/types/IDomainId";
import { EntityFormDialog } from "./EntityFormDialog";
import {
	ConfirmResult,
	FormDialogConfig,
	SelectOption,
} from "./entityDialogTypes";

interface EntityEditDialogProps<
	TEntity extends IDomainId,
	TForm extends FieldValues,
	TUpdate,
> {
	open: boolean;
	// Null hides the dialog entirely.
	entity: TEntity | null;
	config: FormDialogConfig<TForm>;
	// Entity → form values. Must be referentially stable (the entityConfigs
	// exports are module-level, so passing those is always safe).
	toForm: (entity: TEntity) => TForm;
	// Override for join entities whose update DTO carries fields beyond the
	// default `{ id, ...form }` merge (e.g. cupboardInRoom's cupboardId).
	toUpdate?: (form: TForm, entity: TEntity) => TUpdate;
	options?: Record<string, SelectOption[]>;
	staticValues?: Record<string, string>;
	onClose: () => void;
	onConfirm: (dto: TUpdate) => ConfirmResult;
	isLoading: boolean;
}

export const EntityEditDialog = <
	TEntity extends IDomainId,
	TForm extends FieldValues,
	TUpdate = IDomainId & TForm,
>({
	open,
	entity,
	config,
	toForm,
	toUpdate,
	options,
	staticValues,
	onClose,
	onConfirm,
	isLoading,
}: EntityEditDialogProps<TEntity, TForm, TUpdate>) => {
	const initialValues = useMemo(
		() => (entity ? toForm(entity) : null),
		[entity, toForm],
	);

	return (
		<EntityFormDialog
			open={open}
			mode="edit"
			config={config}
			initialValues={initialValues}
			options={options}
			staticValues={staticValues}
			onClose={onClose}
			onConfirm={(form) =>
				onConfirm(
					toUpdate
						? toUpdate(form, entity!)
						: ({ id: entity!.id, ...form } as IDomainId &
								TForm as TUpdate),
				)
			}
			isLoading={isLoading}
		/>
	);
};
