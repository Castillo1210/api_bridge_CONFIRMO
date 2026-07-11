START TRANSACTION;

CREATE TABLE public.plantillas_mensajes_sistema (
    "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "Codigo" character varying(60) NOT NULL,
    "Contenido" text NOT NULL,
    "Descripcion" character varying(200),
    "Activo" boolean NOT NULL,
    "Canal" character varying(20) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
    CONSTRAINT "PK_plantillas_mensajes_sistema" PRIMARY KEY ("Id")
);

CREATE TABLE public.vendedor_messages (
    "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "VendedorId" uuid NOT NULL,
    "SenderType" character varying(20) NOT NULL,
    "SenderId" uuid,
    "Content" text NOT NULL,
    "MessageType" character varying(20) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
    CONSTRAINT "PK_vendedor_messages" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_vendedor_messages_profiles_VendedorId" FOREIGN KEY ("VendedorId") REFERENCES public.profiles ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_plantillas_mensajes_sistema_Codigo_Canal" ON public.plantillas_mensajes_sistema ("Codigo", "Canal");

CREATE INDEX idx_vendedor_messages_vendedor_created ON public.vendedor_messages ("VendedorId", "CreatedAt");

INSERT INTO public.__ef_migrations ("MigrationId", "ProductVersion")
VALUES ('20260711183907_AddChatVendedoresYPlantillasMensajes', '8.0.6');

INSERT INTO public.plantillas_mensajes_sistema (id, codigo, canal, contenido, descripcion, activo, created_at, updated_at)
VALUES
(gen_random_uuid(), 'deposito_confirmado', 'chat', '✅ *DEPÓSITO CONFIRMADO*

🏢 *Empresa:* {{empresa}}
📍 *Sucursal:* {{sucursal}}
🏦 *Banco:* {{banco}}
🔢 *Anexo:* {{anexo}}
📅 *Fecha Depósito:* {{fecha_deposito}}
🆔 *Operación:* {{operacion}}
💰 *Importe:* {{importe}}

El depósito ha sido validado y confirmado exitosamente.

_Mensaje automático del sistema de control de depósitos_', 'Chat al confirmar', true, now(), now()),

(gen_random_uuid(), 'deposito_confirmado', 'push', '{{empresa}} - {{importe}} confirmado correctamente.', 'Push al confirmar', true, now(), now()),

(gen_random_uuid(), 'deposito_rechazado', 'chat', '❌ *DEPÓSITO RECHAZADO*

🏢 *Empresa:* {{empresa}}
🏦 *Banco:* {{banco}}
🆔 *Operación:* {{operacion}}
💰 *Importe:* {{importe}}

📝 *Motivo:* {{observaciones}}

Por favor revisa el depósito y vuelve a intentarlo.

_Mensaje automático del sistema de control de depósitos_', 'Chat al rechazar', true, now(), now()),

(gen_random_uuid(), 'deposito_rechazado', 'push', 'Tu depósito fue rechazado. Motivo: {{observaciones}}', 'Push al rechazar', true, now(), now()),

(gen_random_uuid(), 'deposito_procesado', 'chat', '⏳ *DEPÓSITO EN VALIDACIÓN*

Tu depósito fue procesado correctamente y ya está esperando la confirmación de finanzas.

_Mensaje automático del sistema de control de depósitos_', 'Chat al procesar', true, now(), now()),

(gen_random_uuid(), 'deposito_procesado', 'push', 'Tu depósito fue procesado y espera confirmación de finanzas.', 'Push al procesar', true, now(), now()),

(gen_random_uuid(), 'fecha_antigua', 'chat', '⚠️ Tu depósito quedó registrado con una fecha anterior a hoy. Puede tardar más en validarse.', 'Chat: fecha antigua', true, now(), now()),

(gen_random_uuid(), 'fecha_antigua', 'push', 'Tu depósito tiene fecha antigua, puede tardar más en validarse.', 'Push: fecha antigua', true, now(), now());

COMMIT;

