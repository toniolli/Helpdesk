export const PERIODO_TRABALHO_INICIAL = {
  segundaAtivo: true,
  tercaAtivo: true,
  quartaAtivo: true,
  quintaAtivo: true,
  sextaAtivo: true,
  horaInicioSemana: "",
  horaFimSemana: "",
  sabadoAtivo: false,
  horaInicioSabado: "",
  horaFimSabado: "",
  domingoAtivo: false,
  horaInicioDomingo: "",
  horaFimDomingo: "",
};

const CAMPOS_DIAS_SEMANA = [
  "segundaAtivo",
  "tercaAtivo",
  "quartaAtivo",
  "quintaAtivo",
  "sextaAtivo",
];

const formatarHorarioFormulario = (horario) => {
  if (!horario) return "";

  return String(horario).slice(0, 5);
};

const formatarHorarioApi = (horario) => {
  if (!horario) return null;

  return horario.length === 5 ? `${horario}:00` : horario;
};

export function criarPeriodoFormulario(periodo) {
  if (!periodo) return { ...PERIODO_TRABALHO_INICIAL };

  return {
    segundaAtivo: Boolean(periodo.segundaAtivo),
    tercaAtivo: Boolean(periodo.tercaAtivo),
    quartaAtivo: Boolean(periodo.quartaAtivo),
    quintaAtivo: Boolean(periodo.quintaAtivo),
    sextaAtivo: Boolean(periodo.sextaAtivo),
    horaInicioSemana: formatarHorarioFormulario(periodo.horaInicioSemana),
    horaFimSemana: formatarHorarioFormulario(periodo.horaFimSemana),
    sabadoAtivo: Boolean(periodo.sabadoAtivo),
    horaInicioSabado: formatarHorarioFormulario(periodo.horaInicioSabado),
    horaFimSabado: formatarHorarioFormulario(periodo.horaFimSabado),
    domingoAtivo: Boolean(periodo.domingoAtivo),
    horaInicioDomingo: formatarHorarioFormulario(periodo.horaInicioDomingo),
    horaFimDomingo: formatarHorarioFormulario(periodo.horaFimDomingo),
  };
}

function validarHorario(inicio, fim, periodo) {
  if (!inicio || !fim) {
    throw new Error(`Informe os horários de entrada e saída de ${periodo}.`);
  }

  if (inicio === fim) {
    throw new Error(`A entrada e a saída de ${periodo} não podem ser iguais.`);
  }
}

export function criarPeriodoPayload(periodo) {
  const possuiDiaSemana = CAMPOS_DIAS_SEMANA.some(
    (campo) => periodo[campo],
  );

  if (!possuiDiaSemana && !periodo.sabadoAtivo && !periodo.domingoAtivo) {
    throw new Error("Selecione pelo menos um dia de trabalho.");
  }

  if (possuiDiaSemana) {
    validarHorario(
      periodo.horaInicioSemana,
      periodo.horaFimSemana,
      "segunda a sexta",
    );
  }

  if (periodo.sabadoAtivo) {
    validarHorario(
      periodo.horaInicioSabado,
      periodo.horaFimSabado,
      "sábado",
    );
  }

  if (periodo.domingoAtivo) {
    validarHorario(
      periodo.horaInicioDomingo,
      periodo.horaFimDomingo,
      "domingo",
    );
  }

  return {
    segundaAtivo: periodo.segundaAtivo,
    tercaAtivo: periodo.tercaAtivo,
    quartaAtivo: periodo.quartaAtivo,
    quintaAtivo: periodo.quintaAtivo,
    sextaAtivo: periodo.sextaAtivo,
    horaInicioSemana: possuiDiaSemana
      ? formatarHorarioApi(periodo.horaInicioSemana)
      : null,
    horaFimSemana: possuiDiaSemana
      ? formatarHorarioApi(periodo.horaFimSemana)
      : null,
    sabadoAtivo: periodo.sabadoAtivo,
    horaInicioSabado: periodo.sabadoAtivo
      ? formatarHorarioApi(periodo.horaInicioSabado)
      : null,
    horaFimSabado: periodo.sabadoAtivo
      ? formatarHorarioApi(periodo.horaFimSabado)
      : null,
    domingoAtivo: periodo.domingoAtivo,
    horaInicioDomingo: periodo.domingoAtivo
      ? formatarHorarioApi(periodo.horaInicioDomingo)
      : null,
    horaFimDomingo: periodo.domingoAtivo
      ? formatarHorarioApi(periodo.horaFimDomingo)
      : null,
  };
}
