use actix_web::{get, web, App, HttpResponse, HttpServer, Responder, Result};
use serde::Deserialize;

#[derive(Deserialize)]
struct Info {
    provider: String,
}

struct EncryptedToken {
    redirect: u32,
    signature: [u8; 16],
}

#[get("/")]
async fn home() -> impl Responder {
    HttpResponse::Ok().body("Hello world!")
}

#[get("/exchange/{provider}")]
async fn exchange(info: web::Path<Info>) -> Result<String> {
    Ok(format!("{}", info.provider))
}

#[actix_web::main]
async fn main() -> std::io::Result<()> {
    HttpServer::new(|| {
        App::new()
            .service(home)
            .service(exchange)
    })
    .bind(("127.0.0.1", 8080))?
    .run()
    .await
}