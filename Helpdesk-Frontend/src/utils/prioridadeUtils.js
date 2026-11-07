export function getPrioridade(
    prioridade
) {
    switch (prioridade) {

        case 4:
            return "Crítica";

        case 3:
            return "Alta";

        case 2:
            return "Média";

        default:
            return "Baixa";
    }
}

export function getPrioridadeClass(
    prioridade,
    styles
) {
    switch (prioridade) {

        case 4:
            return styles.critica;

        case 3:
            return styles.alta;

        case 2:
            return styles.media;

        default:
            return styles.baixa;
    }
}