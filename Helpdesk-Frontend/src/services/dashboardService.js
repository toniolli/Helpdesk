import api from "../api/axios";

const dashboardService = {
  obterDashboard: async (filtros = {}) => {
    const params = Object.fromEntries(
      Object.entries(filtros).filter(
        ([, valor]) => valor !== "" && valor !== null && valor !== undefined,
      ),
    );

    const response = await api.get("/Dashboard", { params });

    return response.data;
  },
};

export default dashboardService;
