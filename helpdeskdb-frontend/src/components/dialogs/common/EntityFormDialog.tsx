import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import {
	FieldValues,
	Path,
	RegisterOptions,
	SubmitHandler,
	useForm,
} from "react-hook-form";
import { Modal } from "./Modal";
import {
	ConfirmResult,
	FormDialogConfig,
	SelectOption,
	ValidationSpec,
} from "./entityDialogTypes";

interface EntityFormDialogProps<TForm extends FieldValues> {
	open: boolean;
	mode: "create" | "edit";
	config: FormDialogConfig<TForm>;
	// Edit mode: the values of the entity being edited. Must be
	// referentially stable (memoize in the wrapper) or the form resets
	// on every parent re-render. Null hides the dialog entirely.
	initialValues?: TForm | null;
	// Option lists for kind:"select" fields, keyed by optionsKey.
	options?: Record<string, SelectOption[]>;
	// Values for kind:"readonly" fields, keyed by valueKey.
	staticValues?: Record<string, string>;
	onClose: () => void;
	onConfirm: (data: TForm) => ConfirmResult;
	isLoading: boolean;
}

export const EntityFormDialog = <TForm extends FieldValues>({
	open,
	mode,
	config,
	initialValues,
	options,
	staticValues,
	onClose,
	onConfirm,
	isLoading,
}: EntityFormDialogProps<TForm>) => {
	const { t: tEntity } = useTranslation(config.namespace);
	const { t: tCommon } = useTranslation("common");
	const { t: tValidation } = useTranslation("validationerrors");

	const [errorMsg, setErrorMsg] = useState<string | null>(null);

	const {
		register,
		handleSubmit,
		reset,
		formState: { errors },
	} = useForm<TForm>({
		defaultValues: config.defaultValues,
	});

	// Edit mode: populate the form from the entity being edited.
	useEffect(() => {
		if (open && mode === "edit" && initialValues) {
			reset(initialValues);
		}
	}, [open, mode, initialValues, reset]);

	// Reset when the dialog closes so a persistently mounted dialog
	// (e.g. nested inside CupboardLocationsDialog) opens clean next time.
	useEffect(() => {
		if (!open) {
			reset(config.defaultValues);
			setErrorMsg(null);
		}
	}, [open, reset, config.defaultValues]);

	const handleClose = () => {
		reset(config.defaultValues);
		setErrorMsg(null);
		onClose();
	};

	const onSubmit: SubmitHandler<TForm> = async (data) => {
		setErrorMsg(null);
		const result = await onConfirm(data);
		if (result && result.error) {
			setErrorMsg(result.error);
			return;
		}
		if (mode === "create") {
			reset(config.defaultValues);
		}
	};

	const buildRules = (
		fieldLabel: string,
		validation?: ValidationSpec,
		valueAsNumber?: boolean,
	): RegisterOptions<TForm, Path<TForm>> => {
		const rules: Record<string, unknown> = {};
		if (valueAsNumber) {
			rules.valueAsNumber = true;
		}
		if (validation?.required) {
			rules.required = {
				value: true,
				message: tValidation("Required", { field: fieldLabel }),
			};
		}
		if (validation?.minLength !== undefined) {
			rules.minLength = {
				value: validation.minLength,
				message: tValidation("MinLenghtValidationError", {
					field: fieldLabel,
					min: validation.minLength,
				}),
			};
		}
		if (validation?.maxLength !== undefined) {
			rules.maxLength = {
				value: validation.maxLength,
				message: tValidation("MaxLengthValidationError", {
					field: fieldLabel,
					max: validation.maxLength,
				}),
			};
		}
		if (validation?.min !== undefined) {
			rules.min = {
				value: validation.min,
				message: tValidation("ValueBetween", {
					field: fieldLabel,
					min: validation.min,
					max: validation.max,
				}),
			};
		}
		if (validation?.max !== undefined) {
			rules.max = {
				value: validation.max,
				message: tValidation("ValueBetween", {
					field: fieldLabel,
					min: validation.min,
					max: validation.max,
				}),
			};
		}
		return rules as RegisterOptions<TForm, Path<TForm>>;
	};

	if (mode === "edit" && !initialValues) return null;

	const fieldErrors = errors as unknown as Partial<
		Record<string, { message?: unknown }>
	>;

	return (
		<Modal open={open} onClose={handleClose}>
			<h2 className="text-xl font-bold mb-2 text-black">
				{mode === "create" ? tCommon("CreateTitle") : tCommon("EditTitle")}
			</h2>
			<h4 className="text-lg text-gray-700 mb-4">
				{tEntity(config.singularKey)}
			</h4>

			{errorMsg && (
				<div className="mb-4 p-3 bg-red-100 text-red-700 border border-red-400 rounded">
					{errorMsg}
				</div>
			)}

			<form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
				{config.fields.map((field) => {
					if (field.kind === "display") {
						return (
							<div key={field.valueKey}>
								<span className="block text-xs font-bold uppercase text-gray-500 mb-1">
									{tEntity(field.labelKey)}
								</span>
								<span className="text-gray-900 font-medium">
									{staticValues?.[field.valueKey] ?? ""}
								</span>
							</div>
						);
					}

					if (field.kind === "readonly") {
						return (
							<div key={field.valueKey}>
								<label className="block text-gray-700 mb-2">
									{tEntity(field.labelKey)}
								</label>
								<input
									type="text"
									value={staticValues?.[field.valueKey] ?? ""}
									readOnly
									disabled
									className="w-full p-2 border border-gray-300 rounded bg-gray-100 text-gray-600"
								/>
							</div>
						);
					}

					const fieldLabel = tEntity(field.labelKey);
					const errorMessage = fieldErrors[field.name]?.message as
						| string
						| undefined;

					return (
						<div key={field.name}>
							<label
								htmlFor={field.name}
								className="block text-gray-700 mb-2"
							>
								{fieldLabel}
							</label>

							{field.kind === "text" && (
								<input
									id={field.name}
									type="text"
									className="w-full p-2 border border-gray-300 rounded"
									placeholder={
										field.placeholderKey
											? tEntity(field.placeholderKey)
											: undefined
									}
									{...register(
										field.name,
										buildRules(fieldLabel, field.validation),
									)}
								/>
							)}

							{field.kind === "number" && (
								<input
									id={field.name}
									type="number"
									className="w-full p-2 border border-gray-300 rounded"
									placeholder={field.placeholder}
									{...register(
										field.name,
										buildRules(fieldLabel, field.validation, true),
									)}
								/>
							)}

							{field.kind === "select" && (
								<select
									id={field.name}
									className="w-full p-2 border border-gray-300 rounded"
									{...register(
										field.name,
										field.required
											? ({
													required: tValidation("Required", {
														field: fieldLabel,
													}),
											  } as RegisterOptions<TForm, Path<TForm>>)
											: undefined,
									)}
								>
									<option value="">
										{tCommon(field.selectArticleKey ?? "SelectA")}{" "}
										{fieldLabel}
									</option>
									{(options?.[field.optionsKey] ?? []).map((option) => (
										<option key={option.value} value={option.value}>
											{option.label}
										</option>
									))}
								</select>
							)}

							{errorMessage && (
								<span className="text-red-600 text-sm">
									{errorMessage}
								</span>
							)}
						</div>
					);
				})}

				<div className="flex justify-end gap-3 mt-6">
					<button
						type="button"
						onClick={handleClose}
						disabled={isLoading}
						className="px-4 py-2 bg-gray-400 hover:bg-gray-300 text-white rounded font-medium transition-colors"
					>
						{tCommon("Cancel")}
					</button>
					<button
						type="submit"
						disabled={isLoading}
						className={`px-4 py-2 bg-orange-500 hover:bg-orange-600 font-medium text-white rounded transition-colors ${
							isLoading ? "opacity-50 cursor-not-allowed" : ""
						}`}
					>
						{isLoading
							? tCommon("Processing")
							: mode === "create"
							? tCommon("CreateButton")
							: tCommon("SaveButton")}
					</button>
				</div>
			</form>
		</Modal>
	);
};
