import { Clock3, MoonStar } from "lucide-react";
import styles from "./perfil.module.css";

const DIAS_SEMANA = [
  ["segundaAtivo", "Segunda"],
  ["tercaAtivo", "Terça"],
  ["quartaAtivo", "Quarta"],
  ["quintaAtivo", "Quinta"],
  ["sextaAtivo", "Sexta"],
];

function CamposHorario({
  inicio,
  fim,
  onInicioChange,
  onFimChange,
  disabled,
  identificador,
}) {
  return (
    <div className={styles.scheduleTimeGrid}>
      <div className={styles.field}>
        <label htmlFor={`${identificador}-inicio`}>Entrada</label>
        <input
          id={`${identificador}-inicio`}
          type="time"
          value={inicio}
          onChange={(event) => onInicioChange(event.target.value)}
          disabled={disabled}
          required={!disabled}
        />
      </div>

      <div className={styles.field}>
        <label htmlFor={`${identificador}-fim`}>Saída</label>
        <input
          id={`${identificador}-fim`}
          type="time"
          value={fim}
          onChange={(event) => onFimChange(event.target.value)}
          disabled={disabled}
          required={!disabled}
        />
      </div>
    </div>
  );
}

export default function PeriodoTrabalhoForm({ value, onChange }) {
  const alterarCampo = (campo, novoValor) => {
    onChange({
      ...value,
      [campo]: novoValor,
    });
  };

  const possuiDiaSemana = DIAS_SEMANA.some(([campo]) => value[campo]);

  return (
    <section className={styles.scheduleSection}>
      <div className={styles.scheduleHeader}>
        <div className={styles.scheduleIcon}>
          <Clock3 size={20} />
        </div>
        <div>
          <h3>Período de trabalho</h3>
          <p>Defina os dias e horários usados nos cálculos de SLA.</p>
        </div>
      </div>

      <div className={styles.scheduleBlock}>
        <div className={styles.scheduleBlockHeader}>
          <div>
            <strong>Dias úteis</strong>
            <span>Os dias selecionados compartilham o mesmo horário.</span>
          </div>
        </div>

        <div className={styles.weekDays}>
          {DIAS_SEMANA.map(([campo, label]) => (
            <label
              key={campo}
              className={`${styles.dayOption} ${
                value[campo] ? styles.dayOptionSelected : ""
              }`}
            >
              <input
                type="checkbox"
                checked={value[campo]}
                onChange={(event) => alterarCampo(campo, event.target.checked)}
              />
              <span>{label}</span>
            </label>
          ))}
        </div>

        <CamposHorario
          identificador="semana"
          inicio={value.horaInicioSemana}
          fim={value.horaFimSemana}
          onInicioChange={(horario) =>
            alterarCampo("horaInicioSemana", horario)
          }
          onFimChange={(horario) => alterarCampo("horaFimSemana", horario)}
          disabled={!possuiDiaSemana}
        />
      </div>

      <div className={styles.weekendGrid}>
        <div className={styles.scheduleBlock}>
          <label className={styles.weekendToggle}>
            <input
              type="checkbox"
              checked={value.sabadoAtivo}
              onChange={(event) =>
                alterarCampo("sabadoAtivo", event.target.checked)
              }
            />
            <span>
              <strong>Sábado</strong>
              <small>Ativar expediente</small>
            </span>
          </label>

          <CamposHorario
            identificador="sabado"
            inicio={value.horaInicioSabado}
            fim={value.horaFimSabado}
            onInicioChange={(horario) =>
              alterarCampo("horaInicioSabado", horario)
            }
            onFimChange={(horario) => alterarCampo("horaFimSabado", horario)}
            disabled={!value.sabadoAtivo}
          />
        </div>

        <div className={styles.scheduleBlock}>
          <label className={styles.weekendToggle}>
            <input
              type="checkbox"
              checked={value.domingoAtivo}
              onChange={(event) =>
                alterarCampo("domingoAtivo", event.target.checked)
              }
            />
            <span>
              <strong>Domingo</strong>
              <small>Ativar expediente</small>
            </span>
          </label>

          <CamposHorario
            identificador="domingo"
            inicio={value.horaInicioDomingo}
            fim={value.horaFimDomingo}
            onInicioChange={(horario) =>
              alterarCampo("horaInicioDomingo", horario)
            }
            onFimChange={(horario) => alterarCampo("horaFimDomingo", horario)}
            disabled={!value.domingoAtivo}
          />
        </div>
      </div>

      <div className={styles.nightShiftHint}>
        <MoonStar size={17} />
        <span>
          Para turno noturno, informe uma saída menor que a entrada. Exemplo:
          18:00 até 03:00.
        </span>
      </div>
    </section>
  );
}
