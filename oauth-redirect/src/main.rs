use actix_web::{get, post, web, App, HttpResponse, HttpServer, Responder, Result};
use serde::Deserialize;

#[derive(Deserialize)]
struct Info {
    provider: String,
}

#[derive(Deserialize)]
struct AllowList {
    state: String,
}

struct redirectString {
    exp_at: u32,
    state: u16,
}

#[get("/")]
async fn home() -> impl Responder {
    HttpResponse::Ok().body("Hello world!")
}

#[get("/exchange/{provider}")]
async fn exchange(info: web::Path<Info>) -> Result<String> {
    Ok(format!("{}", info.provider))
}

#[post("/exchange/allow/{state}")]
async fn allow(info: web::Path<AllowList>) -> Result<String> {
    Ok(format!("OK"))
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