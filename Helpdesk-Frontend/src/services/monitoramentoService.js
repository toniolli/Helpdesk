import api from "../api/axios";

const monitoramentoService = {
  obterPainel: async () => {
    const { data } = await api.get("/Monitoramento/painel");
    return data;
  },
};

export default monitoramentoService;
