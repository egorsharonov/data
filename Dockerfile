FROM sregistry.mts.ru/salsa/ci-cd/base-image-runtime-dotnet8:latest

ARG ARTIFACTS_PATH='publish'

RUN addgroup --system nonroot \
    && adduser --system --ingroup nonroot nonroot
USER nonroot

WORKDIR /app
COPY ${ARTIFACTS_PATH} .
ENTRYPOINT ["dotnet", "Informing.Data.Worker.dll"]