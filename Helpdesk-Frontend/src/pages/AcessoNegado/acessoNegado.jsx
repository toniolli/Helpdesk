import styles from "./acessoNegado.module.css";

export default function AcessoNegado() {
  return (
    <section className={styles.container}>
      <span className={styles.label}>ACESSO RESTRITO</span>
      <h1>Você não possui permissão para acessar esta área.</h1>
      <p>Use o menu lateral para acessar uma das áreas disponíveis para você.</p>
    </section>
  );
}
