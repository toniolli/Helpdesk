const getCssColor = (variable) =>
    getComputedStyle(document.documentElement)
        .getPropertyValue(variable)
        .trim();

export const chartColors = {
    primary: () => getCssColor("--primary"),
    primaryLight: () => getCssColor("--primary-light"),

    success: () => getCssColor("--success"),
    danger: () => getCssColor("--danger"),
    warning: () => getCssColor("--warning"),

    secondary: () => getCssColor("--secondary")
};

export const statusColors = {
    Aberto: () => chartColors.warning(),
    EmAtendimento: () =>
        chartColors.primaryLight(),
    Fechado: () => chartColors.success(),
    Cancelado: () => chartColors.danger(),
    AguardandoRetorno: () => chartColors.secondary(),
    Distribuido: () => chartColors.primary(),
    AguardandoEquipamento: () => chartColors.warning()
};

export const prioridadeColors = {
    Baixa: () => chartColors.secondary(),
    Média: () => chartColors.primaryLight(),
    Alta: () => chartColors.warning(),
    Crítica: () => chartColors.danger()
};
